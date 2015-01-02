******************Installation Instructions******************
If you are currently using an older version of CactEye, then please delete CactEye from your GameData
folder before installing a new version.

Extract the enclosed files to your GameData folder. 

Included is another folder entitled "Compatibility Patch for Distant Object v1.5.1," this is needed
only if you are running Distant Object v1.5.1. The contents of that folder contains a compatibility 
patch that makes CactEye 2 fully compatible with Distant Object 1.5.1. If you are not using 
Distant Object, then the patch is not needed and will introduce unneccesary log spam. 

To install the Compatibility patch for Distant Object Enhancement, extract the contents of the 
"Compatibility Patch for Distant Object v1.5.1" folder to your GameData folder.

******************Change Log******************

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