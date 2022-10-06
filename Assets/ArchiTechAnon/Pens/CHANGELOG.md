# ArchiTech AirPens Changelog

## v0.8
- Fix eraser performance issues, switched to physics trigger setup
- Add Misc Menu toggle to visually show the eraser collider points
- Fix built-in pointer colliding with the eraser joints when ink is in between the hand an the UI
- Fix being able to pick up other pens when beloning to a protected pool

## v0.7
- Add ink eraser functionality for individual ink lines instead of just clearing all.
- Partially restructure the prefabs. Toggle/Clear UI elements are now individually part of the SinglePen prefab.
- Adjust transform positioning of some elements to make them more uniform relative to the prefab root.
- Improve stability of variable synchronization logic between owner/clients.
- Increase reliability of intended action when drawing of dots vs opening menu.
- Add new Misc menu with initial option of stright line drawing mode.
- Add back button to menus to navigate around without closing the menu.

## v0.6
- Fixes issue where pen was unable to draw dots due to min draw length conflicting with the menu double click logic. Can now do dots if the pen is held in the same place for the length of the double click threshold (default is 400ms).
- Adds new flags to the pen sets prefabs for the following:
    - Allowing a player to own multiple pens of the affected group.
    - Allowing a player to take ownership of someone else's pen while dropped or held. This is 2 separate settings (While Dropped and While Held) that can be enabled independently for more specific ownership control model.
- Updated prefabs with the most common combinations of a protected set (the Exclusive Ownership setup that this asset was designed for originally) and a more typical 'open use' mode, akin to how most other pen prefabs work.
- Fixes issue where drawing dots only worked 50% of the time
- Adds slight performance optimizations related to the sync variables

## v0.5
- Updated menus into a new unified flow. Just a double click to open the unified menu which enables direct access to the sub-menus without remembering different click combos.
- Added mitigations for accidental menu opening while doing short rapid bursts of drawing short lines. Should help reduce frustration while drawing.

## v0.4
- Minor adjustments to writing.
- Fix some interactions that were broke (blue pen, auto-enable on join not working, etc)
- Minor adjustments to the UIs trying to make the UX a bit more bearable prior to the next redesign. 
- Made the pens automatically pick a random color on world init to make them more interesting.
- Players have on multiple occasions accidentally toggled another's pen, unintentionally clearing the ink, so I've updated ink to NOT clear when hiding. Clearing someone else's ink (locally only) should require intention, and so is now limited to explicitly clicking the trash icon next to their wall tag.

## v0.3
- First Beta release
