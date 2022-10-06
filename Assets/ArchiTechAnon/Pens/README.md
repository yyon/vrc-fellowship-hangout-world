# ArchiTechAnon AirPens Prefab
## Requirements
- Ensure latest VRCSDK3 (Udon) is imported (last tested with v2021.3.22.18.27 aka v2021.1.5)
- Ensure latest UdonSharp version is imported (last tested with [v0.19.8](https://github.com/MerlinVR/UdonSharp/releases/download/v0.19.8/UdonSharp_v0.19.8.unitypackage))
- Import this package
- Drag the desired prefab (located at `Assets->ArchiTechAnon->Pens->*.prefab`) into your scene wherever you like, rotate as needed.
- Done.

## Features
- Position-based draw synchronization
- Customizable ownership management for world creators
- Unified menu to access all pen options, opens with just a double click while holding the pen
- Color picker menu to swap between up to 10 stored colors (can change them at any time)
- Brush picker menu to adjust the level of detail (line segment length) and the draw width (thicc-ness of the line)
- Clear all ink of a pen by holding it backwards (pen tip facing towards you) and double click
- Clear parts of the ink by holding it backwards and holding down the use button to highlight ink lines. Release to erase.
- Has various modes: Exclusive ownership, owning multiple, theft while held and/or dropped
- Can hide/show/clear ink individually per pen
- In the pen set prefabs, there are management interactables to handle respawning and releasing pens within the group, as well as UI elements to help players manage their local interaction/view with the pen.
- Only uses Unity built-in shaders

## Customization
- To start off, there are three prefabs that come with this package. The simplest one is the basic `SinglePen` prefab. 
This is a self-contained instance of a pen that can be simply dropped in a world to be available for use.
- On this prefab, there are two settings available. `Theft Allowed While Dropped` and `Theft Allowed While Held`. 
These flags determine whether or not another player is allowed to steal an pen someone else is using (read: is the owner of).
- The `While Dropped` allows another player to take the pen that the owner has _intentionally_ dropped.
The `While Held` allows another player to take the pen that the owner has _while it is in use_ by the player.
- These settings are independent, so you can choose whichever combination you want for your world.
- The second and third prefabs are nearly identical. The `Protected Pens` defaults to what is called `Exclusive Ownership` mode.
- This mode means that any pens in the group can only be owned by one player for the length of time that the player is in the instance, as well as each player can only own one pen at a time.
    This mode was developed to help reduce pen spam in worlds as well as guarenteeing player's the ability to retain 'their' pen as long as they are in the world. (I've witnessed many times a mute was wanting to use a pen, but some desktop user took it.)
- The `Open Pens` prefab has the `Theft Allowed While Dropped` enabled as well as an option only available on the pen set prefabs, `Allow Owning Multiple`.
- The `Allow Owning Multiple` flag name is pretty clear, but what it does is allows any player to pick up any number of pens that is within the pen group.
- You can freely mix and match between the settings on the pen set prefabs for your desired ownership model. 
You can find these settings on the `PenPool` node in the prefab under the `PoolManager` script.
- If you want to add more pens to the pen set prefabs, simply drop on of the prefabs into the world, open up the `PenPool` node heirarchy and add additional SinglePen prefabs under it.
