using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{

    public class CactEyeOptics: PartModule
    {
        [KSPField(isPersistant = false)]
        public bool DebugMode = false;

        [KSPField(isPersistant = false)]
        public bool IsSmallOptics = false;

        [KSPField(isPersistant = false)]
        public string CameraTransformName = "CactEyeCam";

        [KSPField(isPersistant = false)]
        public bool IsFunctional = false;

        [KSPField(isPersistant = true)]
        public bool IsDamaged = false;

        [KSPField(isPersistant = true)]
        public bool SmallApertureOpen = false;

        private ModuleAnimateGeneric opticsAnimate;

        //Control Variable that disables functionality if there is an error.
        //private bool Error = false;

        private TelescopeMenu TelescopeControlMenu;


        /*
         * Function name: OnStart
         * Purpose: This overrides the OnStart functionality. This function will be called once
         * at the start of the game directly after a scene load. In this case, the function will
         * instatiate the GUI.
         */
        public override void OnStart(StartState state)
        {

            //tie-in with Firespitter.
            opticsAnimate = GetComponent<ModuleAnimateGeneric>();

            //Attempt to instantiate the GUI
            Transform temp = part.FindModelTransform(CameraTransformName);
            try
            {
                TelescopeControlMenu = new TelescopeMenu(temp);
            }
            catch (Exception E)
            {
                //Error = true;
                Debug.Log("CactEye 2: Exception 1: Was not able to create the Telescope Control Menu object. You should try re-installing CactEye2 and ensure that old versions of CactEye are deleted.");
                Debug.Log(E.ToString());
                Debug.Log(temp.ToString());
            }
        }

        public override void OnUpdate()
        {

            //Enable Repair Scope context menu option if the scope is damaged.
            if (IsDamaged)
            {
                IsFunctional = false;
                Events["FixScope"].active = true;
            }

            //If the scope isn't damage, then toggle scope functionality based on the aperture.
            else
            {
                if (opticsAnimate != null)
                {
                    if (opticsAnimate.animTime < 0.5 && IsFunctional)
                    {
                        IsFunctional = false;
                    }
                    if (opticsAnimate.animTime > 0.5 && !IsFunctional)
                    {
                        IsFunctional = true;
                    }
                }
            }

            //Send updated position information to the telescope gui object.
            TelescopeControlMenu.UpdatePosition(part.FindModelTransform(CameraTransformName));
        }

        public void BreakScope()
        {
            IsFunctional = false;
            IsDamaged = true;
        }

        public bool IsMenuEnabled()
        {
            try
            {
                return TelescopeControlMenu.IsMenuEnabled();
            }
            catch
            {
                Debug.Log(" CactEye 2: Unknown Exception");
                return false;
            }
        }

        public float GetFOV()
        {
            return TelescopeControlMenu.GetFOV();
        }

        [KSPEvent(guiActive = true, guiName = "Control from Here", active = true)]
        public void controlFromHere()
        {
            vessel.SetReferenceTransform(part);
        }

        [KSPEvent(guiActive = true, guiName = "Toggle GUI", active = true)]
        public void ToggleGUI()
        {
            try
            {
                TelescopeControlMenu.Toggle();
            }
            catch (Exception E)
            {
                Debug.Log("CactEye 2: Exception 3: Was not able to bring up the Telescope Control Menu. The Telescope Control Menu returned a null reference.");
                Debug.Log(E.ToString());
            }
        }

        [KSPEvent(active = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repair Optics", unfocusedRange = 5)]
        public void FixScope()
        {
            IsDamaged = false;
            Events["FixScope"].active = false;
        }

        [KSPEvent(active = false, guiActive = true, guiActiveUnfocused = true, guiName = "Open Aperture (permanent!)", unfocusedRange = 2)]
        public void OpenSmallAperture()
        {
            SmallApertureOpen = true;
            Events["OpenSmallAperture"].active = false;
        }
    }
}
