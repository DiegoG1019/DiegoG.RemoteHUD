The first thing that needs to be implemented is a simple, predetermined and unremovable UI layout. This will remain for testing purposes and PoC

I've opted against having the server set the position of HUD elements; instead, the server will only manage state and existence

<ul>Maybe we could implement a button to force positions, and that server-side positions are assumed whenever a new one is added? Whenever a new one is added, it shouldn't move the other stuff.</ul>

