******************Installation Instructions******************
If you are currently using an older version of CactEye, then please delete CactEye from your GameData
folder before installing a new version.

Extract the enclosed files to your GameData folder. 

Included is another folder entitled "Compatibility Patch for Distant Object v1.5.4," this is needed
only if you are running Distant Object v1.5.4. The contents of that folder contains a compatibility 
patch that makes CactEye 2 fully compatible with Distant Object 1.5.4. If you are not using 
Distant Object, then the patch is not needed and will introduce unnecessary log spam. 

NOTE: Distant Object Enhancement has been temporarily removed
To install the Compatibility patch for Distant Object Enhancement, extract the contents of the 
"Compatibility Patch for Distant Object v1.5.4" folder to your GameData folder.

******************Change Log******************
CactEye 2 1.2.0.9
-Recompiled against 1.2.0
-Restructured CactEye GUI for useability

CactEye 2 1.1.3.8
-Temporary Removal of DOE patch
-Science definitions overhauled to clean them up
-Science rebalance
-Science Changes
--Processor Level Multipliers
---Level 1 25%
---Level 2 50%
---Level 3 100%
--Telescope Multipliers
---Fungeye 50%
---CactEye 100%
--Added adaptive science cap based on processor level and telescope body
-Update contracts to match new science

CactEye 2 1.1.3.7
-Fixed Dres science bug again

CactEye 2 1.1.3.6
-Research Bodies integration activated

CactEye 2 1.1.3.5
-Fixed Dres science bug
-Config file settings should be honored
-Rebalance of science
-Telescopes only operate in Kerbin high orbit
-Updated compile against DOE
-Initial Research Bodies integration

CactEye  2 1.1.3.4
-Compiled against 1.1.3
-Updated science to be all from high Kerbin orbit to prevent progression contract breaking

CactEye 2 1.1.3.3
-Initial science move to high Kerbin orbit

CactEye 2 1.1.2.1
-Initial Release by IceDown
-Compiled against 1.1.2
-Fixed GUI lockup while doing science

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