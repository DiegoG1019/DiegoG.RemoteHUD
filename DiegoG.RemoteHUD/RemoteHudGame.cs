using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiegoG.MonoGame.DependencyInjection;
using DiegoG.MonoGame.Extended;
using DiegoG.MonoGame.Extended.Tasks;
using DiegoG.MonoGame.Extended.UIComponents;
using DiegoG.RemoteHud;
using DiegoG.RemoteHud.HudManagers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using MonoGame.ImGuiNet;

namespace DiegoG.RemoteHud;

public class RemoteHudGame : Game
{
    public CancellationToken StoppingToken { get; set; }
    public GraphicsDeviceManager Graphics { get; }
    public StatefulSpriteBatch SpriteBatch { get; private set; } = null!;
    public SpriteFont DebugFont { get; private set; } = null!;
    public CallDeferrer Deferrer { get; }
    public MouseStateMemory MouseState { get; private set; }
    public KeyboardStateExtended KeyboardState { get; private set; }
    public Scene MainMenuScene { get; }
    public GameServiceProvider GameServices { get; private set; } = null!;

    private ImGuiRenderer imGuiRenderer = null!;

    public HudManager? HudManager
    {
        get;
        set
        {
            Components.Remove(field);
            if (value is not null)
            {
                if (value.Game != this)
                    throw new ArgumentException("A HudManager assigned to a game must have been constructed for that game", nameof(value));
                Components.Add(value);
            }
            field = value;
        }
    }

    public RemoteHudGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        MainMenuScene = new(this);
        Deferrer = Services.AddCallDeferrerService(this);
        Components.Add(new BackgroundTasksWorkerComponent(this));
        Services.AddStringBuilderPool();
    }

    protected override void Initialize()
    {
        base.Initialize();
        SpriteBatch = new StatefulSpriteBatch(GraphicsDevice);
        MainMenuScene.Initialize();
        
        Window.AllowUserResizing = true;
        Window.Title = "Remote HUD - By Diego García";
        
        imGuiRenderer = new ImGuiRenderer(this);
        imGuiRenderer.RebuildFontAtlas();

        DebugFont = Content.Load<SpriteFont>("Fonts/CascadiaMono");
        
        Components.Add(new DebugImGuiViews(this));
        
        GameServices = new GameServiceProvider(this, services =>
        {
            services.AddSingleton(this);
            services.AddSingleton(SpriteBatch);
            services.AddSingleton<SpriteBatch>(SpriteBatch);
            services.AddTransient<HudManager>(s => s.GetRequiredService<RemoteHudGame>().HudManager!);
            foreach (var (id, element) in HudElementStore.Elements)
                services.AddTransient(element.ElementType);
        });
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (StoppingToken.IsCancellationRequested)
            return;

        Deferrer.ExecuteUpdateStartDeferredCalls(gameTime);
        
        if (HudManager is null)
            MainMenuScene.Update(gameTime);
        base.Update(gameTime);
        
        Deferrer.ExecuteUpdateEndDeferredCalls(gameTime);
        MouseState = new MouseStateMemory(MouseState, Mouse.GetState());
        KeyboardExtended.Update();
        KeyboardState = KeyboardExtended.GetState();
    }

    protected override void Draw(GameTime gameTime)
    {
        if (StoppingToken.IsCancellationRequested)
            return;

        GraphicsDevice.Clear(Color.Black);
        Deferrer.ExecuteDrawStartDeferredCalls(gameTime);

        SpriteBatch.BeginWithState();
        imGuiRenderer.BeginLayout(gameTime);
        
        if (HudManager is null)
            MainMenuScene.Draw(gameTime);
        base.Draw(gameTime);

        Deferrer.ExecuteDrawEndDeferredCalls(gameTime);
        
        SpriteBatch.End();
        imGuiRenderer.EndLayout();
    }
}
