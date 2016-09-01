using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    public class CactEyeAsteroidProcessor: CactEyeProcessor
    {
        //Needs to be persistent to allow AsteroidSpawner to pull data.
        [KSPField(isPersistant = true)]
        public int DiscoveryRate = 10;

        [KSPField(isPersistant = false)]
        public bool ScannerActive = false;

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Engage Asteroid Scanner", unfocusedRange = 2)]
        public void EngageScanner()
        {
            ScannerActive = true;
            ActivateProcessor();
            Events["DisengageScanner"].active = true;
            Events["EngageScanner"].active = false;
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Disengage Asteroid Scanner", unfocusedRange = 2)]
        public void DisengageScanner()
        {
            ScannerActive = false;
            DeactivateProcessor();
            Events["DisengageScanner"].active = false;
            Events["EngageScanner"].active = true;
        }

        public float GetDiscoveryRate()
        {
            return DiscoveryRate;
        }

        /* ************************************************************************************************
         * Function Name: DeactivateProcessor
         * Input: None
         * Output: None
         * Purpose: This function will allow other classes to deactivate the processor. 
         * ************************************************************************************************/
        public override void DeactivateProcessor()
        {

            if (ScannerActive)
            {
                return;
            }

            else
            {
                Active = false;

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Processor deactivated!");
                }
            }

            //RevertLightDirection();
        }

        public override string DoScience(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {

            Vessel TargetVessel = FlightGlobals.fetch.VesselTarget.GetVessel();

            //If target is not an asteroid
            if (TargetVessel == null || TargetVessel.vesselType != VesselType.SpaceObject)
            {
                return Type + ": Invalid target type!";
            }

            //if target is not in telescope view
            else if (TargetPosition == new Vector3(-1, -1, 0))
            {
                return Type + ": Target not in scope field of view.";
            }

            else if (FOV > 0.5f)
            {
                return Type + ": Scope not zoomed in far enough.";
            }

            else if (CactEyeAPI.CheckOccult(TargetVessel) != "")
            {
                return Type + ": Target is occulted by another body.";
            }

            else
            {
                float SciencePoints = 0f;
                string TargetName = TargetVessel.name;
                ScienceExperiment AsteroidExperiment = ResearchAndDevelopment.GetExperiment(ExperimentID);
                ScienceSubject AsteroidSubject = ResearchAndDevelopment.GetExperimentSubject(AsteroidExperiment, ExperimentSituations.InSpaceHigh, FlightGlobals.ActiveVessel.mainBody, "");

                SciencePoints += AsteroidExperiment.baseValue * AsteroidExperiment.dataScale * maxScience;

                //These two lines cause a bug where the experiment gives an infinite supply of science points.
                //WideFieldSubject.scientificValue = 1f;
                //WideFieldSubject.science = 0f;

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: SciencePoints: " + SciencePoints.ToString());
                }

                //Different scopes have different multipliers for the science gains.
                SciencePoints *= scienceMultiplier;

                ScienceData Data = new ScienceData(SciencePoints, 1f, 0f, AsteroidSubject.id, Type + " " + TargetName + " Observation");
                StoredData.Add(Data);
                ReviewData(Data, Screenshot);
                return "";
            }
        }

        public override void OnStart(StartState state)
        {
            Events["EngageScanner"].active = true;
        }

        public override Texture2D ApplyFilter(string Filter, Texture2D InputTexture)
        {

            Color[] Colors = InputTexture.GetPixels();

            for (int i = 0; i < Colors.Length; i++)
            {
                float GrayscaleValue = Colors[i].grayscale;
                Colors[i] = new Color(GrayscaleValue, GrayscaleValue, GrayscaleValue, Colors[i].a);
            }

            InputTexture.SetPixels(Colors);

            return InputTexture;
        }
    }
}
