﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    class CactEyeWideField: CactEyeProcessor
    {

        /* ************************************************************************************************
         * Function Name: DoScience
         * Input: Position of the target, whether or not we're dealing with the FungEye or CactEye optics,
         *          the current field of view, and a screenshot.
         * Output: None
         * Purpose: This function will generate a science report based on the input parameters. This is an 
         * override of a function prototype. This will generate a science report based on the target 
         * celestial body. Science reports will only be generated if the target is a celestial body,
         * if the target is not the sun, if the target is visible in the scope, and if the telescope
         * is zoomed in far enough.
         * ************************************************************************************************/
        public override string DoScience(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            CelestialBody Target = FlightGlobals.Bodies.Find(n => n.GetName() == FlightGlobals.fetch.VesselTarget.GetName());
            CelestialBody Home = this.vessel.mainBody;

            //Sandbox or Career mode logic handled by gui.
            //if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            //{
            //    //CactEyeGUI.DisplayText("Science experiments unavailable in sandbox mode!");
            //    return;
            //}
            if (FlightGlobals.fetch.VesselTarget.GetType().Name != "CelestialBody")
            {
                //Invalid target type
                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Wide Field Camera: Invalid Target Type.");
                }
                return Type + ": Invalid Target Type.";
            }
            else if (Target == FlightGlobals.Bodies[0])
            {
                //Cannot target the sun
                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Wide Field Camera: Cannot target the sun.");
                }
                return Type + ": Cannot target the sun.";
            }
            else if (TargetPosition == new Vector3(-1, -1, 0))
            {
                //target not in scope
                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Wide Field Camera: Target not in scope.");
                }
                return Type + ": Target not in scope field of view.";
            }

            //This has a tendency to be rather tempermental. If a player is getting false "Scope not zoomed in far enough" errors,
            //then the values here will need to be adjusted.
            else if (FOV > CactEyeAPI.bodySize[Target] * 50f)
            {
                //Scope not zoomed in far enough
                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Wide Field Camera: Scope not zoomed in far enough.");
                    Debug.Log("CactEye 2: Wide Field Camera: " + FOV.ToString());
                    Debug.Log("CactEye 2: Wide Field Camera: " + (CactEyeAPI.bodySize[Target] * 50f).ToString());
                }
                return Type + ": Scope not zoomed in far enough.";
            }

            //Check to see if target is blocked.
            else if (CactEyeAPI.CheckOccult(Target) != "")
            {
                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Target is occulted by another body.");
                }
                return Type + ": Target is occulted by another body.";
            }

            else
            {

                float SciencePoints = 0f;
                float ScienceAdjustedCap = 0f;
                float ScienceAvailableCap = 0f;
                string TargetName = Target.name;
                ScienceExperiment WideFieldExperiment;
                ScienceSubject WideFieldSubject;

                bool withParent;
                CelestialBody parentBody;
                

                ExperimentID = "CactEyePlanetary";
                try
                {
                    WideFieldExperiment = ResearchAndDevelopment.GetExperiment(ExperimentID);
                    WideFieldSubject = ResearchAndDevelopment.GetExperimentSubject(WideFieldExperiment, ExperimentSituations.InSpaceHigh, Target, "VisualObservation" + Target.name);
                    WideFieldSubject.title = "CactEye Visual Planetary Observation of " + Target.name;
                    SciencePoints = WideFieldExperiment.baseValue * WideFieldExperiment.dataScale * maxScience * scienceMultiplier;
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("Cacteye 2: SciencePoints: " + SciencePoints);
                        Debug.Log("Cacteye 2: Current Science: " + WideFieldSubject.science);
                        Debug.Log("Cacteye 2: Current Cap: " + WideFieldSubject.scienceCap);
                        Debug.Log("Cacteye 2: ScienceValue: " + WideFieldSubject.scientificValue);
                        Debug.Log("Cacteye 2: SubjectValue: " + ResearchAndDevelopment.GetSubjectValue(SciencePoints, WideFieldSubject));
                        Debug.Log("Cacteye 2: RnDScienceValue: " + ResearchAndDevelopment.GetScienceValue(SciencePoints, WideFieldSubject, 1.0f));
                        Debug.Log("Cacteye 2: RnDReferenceDataValue: " + ResearchAndDevelopment.GetReferenceDataValue(SciencePoints, WideFieldSubject));

                    }
                    //Modify Science cap and points gathered based on telescope and processor
                    ScienceAdjustedCap = WideFieldExperiment.scienceCap * WideFieldExperiment.dataScale * maxScience * scienceMultiplier;
                    
                    //Since it's not clear how KSP figures science points, reverse engineer based off of what this will return.
                    ScienceAvailableCap = ScienceAdjustedCap - ((SciencePoints / ResearchAndDevelopment.GetScienceValue(SciencePoints, WideFieldSubject, 1.0f)) * WideFieldSubject.science);
                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("Cacteye 2: Adjusted Cap: " + ScienceAdjustedCap);
                        Debug.Log("Cacteye 2: Available Cap: " + ScienceAvailableCap);
                    }
                    if (ScienceAvailableCap < 0)
                    {
                        ScienceAvailableCap = 0;
                    }
                    if (SciencePoints > ScienceAvailableCap)
                    {
                        SciencePoints = ScienceAvailableCap;
                    }
                    

                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: SciencePoints: " + SciencePoints.ToString());
                    }


                    ScienceData Data = new ScienceData(SciencePoints, 1f, 0f, WideFieldSubject.id, WideFieldSubject.title);
                    StoredData.Add(Data);
                    ReviewData(Data, Screenshot);
                    if (RBWrapper.APIRBReady)
                    {
                        Debug.Log("CactEye 2: Wrapper ready");
                        
                        RBWrapper.CelestialBodyInfo cbi;

                        RBWrapper.RBactualAPI.CelestialBodies.TryGetValue(Target, out cbi);
                        if(!cbi.isResearched) 
                        {
                            int RBFoundScience = (int)(8f * WideFieldExperiment.dataScale);
                            RBWrapper.RBactualAPI.FoundBody(RBFoundScience, Target, out withParent, out parentBody);
                        }
                        else 
                        {
                            System.Random rnd = new System.Random();
                            RBWrapper.RBactualAPI.Research(Target, rnd.Next(1,11));
                        }
                        

                    }
                    else
                    { 
                        Debug.Log("CactEye 2: Wrapper not ready");
                    }
                }

                catch (Exception e)
                {
                    Debug.Log("CactEye 2: Excpetion 5: Was not able to find Experiment with ExperimentID: " + ExperimentID.ToString());
                    Debug.Log(e.ToString());

                    return "An error occurred. Please post on the Official CactEye 2 thread on the Kerbal Forums.";
                }

                return "";
            }
        }
    }
}
