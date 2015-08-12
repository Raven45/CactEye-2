******************Installation Instructions******************
If you are currently using an older version of CactEye, then please delete CactEye from your GameData
folder before installing a new version.

Extract the enclosed files to your GameData folder. 

Included is another folder entitled "Compatibility Patch for Distant Object v1.5.4," this is needed
only if you are running Distant Object v1.5.4. The contents of that folder contains a compatibility 
patch that makes CactEye 2 fully compatible with Distant Object 1.5.4. If you are not using 
Distant Object, then the patch is not needed and will introduce unnecessary log spam. 

To install the Compatibility patch for Distant Object Enhancement, extract the contents of the 
"DistantObjectHook" folder to your GameData folder.

******************Change Log******************

CactEye2 BETA 6.2
- Recompiled for the latest version of DOE
- The GRU-1000 now has a lifespan of 100 days.
- The GRU-2000 now has a lifespan of 200 days.

CactEye 2 BETA 6.1
NOTE: Angel-125 here, raven appears to have been away for a couple of months, so I've forked his build of CactEye and am doing some caretaking/maintennance until raven gets back. I'm hoping he/she will return to a bunch of pull requests from me and pick up the mod again. If not, I'll do what I can to continue the er, continuation of RubberDucky's mod.

- Recompiled for KSP 1.0.4
- Updated parts to latest KIS/KAS

CactEye 2 BETA 6
-Fixed an issue where the skybox would not be visible in pictures taken with a CactEye telescope.
-Fixed a timewarp issue with the skybox.
-Fixed an issue where the FungEye telescope would not explode when pointed at the sun.
-Added contracts!
-Added an example craft of the small telescope.

CactEye 2 BETA 5.2
-Fixed attachment node issues on some of the structural parts.
-Added an example of the large telescope per a request on the forums.

CactEye 2 BETA 5.1
-Compatibility update for KSP 1.0
-Compatibility update for Distant Object Enhancement 1.5.4
-Added support for Outer Planets Mod, thanks to ImAHungryMan!
-Removed the CactEye agency, for now. This was to prevent KSP from breaking if CactEye is removed. 
This could break existing save games, you have been warned. The agency will be added back in a future
update as other modders and I figure out a better way to manage custom agencies without ruining 
people's save games.

CactEye 2 BETA 5
-Fixed an issue where the telescope would throw an incorrect "out of power" message when the telescope was 
equipped with both an asteroid and a wide field processor. 
-Fixed an issue where the probe core did not have a SAS module, again...
-Fixed an issue where the processors do not consume power. 
-Added Remote Tech support to the probe core.
-Added KSP-AVC support.

CactEye 2 BETA 4
-Fixed an issue where the science dialog would not reset after running an experiment with a CactEye processor;
the symptom of this was a CactEye screenshot showing in a science dialog after running an unrelated experiment.

CactEye 2 BETA 3
-Fixed an issue where running a science experiment with either the "Wide Field Camera 1" or
"Wide Field Camera 2" would produce a "NullReferenceException."
-Fixed an issue where attempting to switch to another processor would grey the GUI out with the error
message that the telescope was out of power, even with full batteries. 
-Fixed a "ArgumentOutOfRange" exception that would be thrown while switching to another processor.
-Fixed an issue with both the Wide Field Camera processors and the Asteroid Camera processors where an 
incorrect amount of science points would be awarded. 

CactEye 2 BETA 2
-Completely rewritten code base for CactEye. 
-Completely redesigned GUI for the Telescope controls. 
-Asteroid telescopes should no longer be restricted to Kerbin orbit.
-Occulation experiments are not yet available.