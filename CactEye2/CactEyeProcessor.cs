using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    abstract public class CactEyeProcessor: PartModule
    {
        [KSPField(isPersistant = false)]
        public string Type = "Default Processor";

        [KSPField(isPersistant = false)]
        public float MinimumFOV = 0.1f;

        [KSPField(isPersistant = false)]
        public float maxScience = 0.25f;

        [KSPField(isPersistant = false)]
        public float consumeRate = 2f;

        [KSPField(isPersistant = false)]
        public string ExperimentID = "Default Experiment";

        [KSPField(isPersistant = true)]
        public bool Active = false;

        protected List<ScienceData> StoredData = new List<ScienceData>();

        private Vector3d OriginalSunDirection;

        private GUIStyle ScienceStyle;
        private GUIStyle ProgressStyle;
        private GUISkin SkinStored;
        private GUIStyleState StyleDefault;

        /* ************************************************************************************************
         * Function Name: GetProessorType
         * Input: None
         * Output: The internal variable "Type"
         * Purpose: This function will return the type of processor that another class is working with.
         * The variable "Type" is a variable that can be modified through the part config files, and 
         * specifies what type of processor the part is. This type will determine how the class behaves.
         * ************************************************************************************************/
        public string GetProcessorType()
        {
            return Type;
        }

        /* ************************************************************************************************
         * Function Name: GetMinimumFOV
         * Input: None
         * Output: The internal variable "MinimumFOV"
         * Purpose: This function will return the variable "MinimumFOV," of which is specified by an
         * external part config file. The MinimumFOV determines the maximum zoom that a telescope is 
         * capable of. This function allows another class to retrieve that value.
         * ************************************************************************************************/
        public float GetMinimumFOV()
        {
            return MinimumFOV;
        }

        /* ************************************************************************************************
         * Function Name: GetActive
         * Input: None
         * Output: true/false
         * Purpose: This function will return a value stating where or not the processor in question
         * is active and is consuming electricity from the craft.
         * ************************************************************************************************/
        public bool IsActive()
        {
            return Active;
        }

        /* ************************************************************************************************
         * Function Name: Die
         * Input: None
         * Output: None
         * Purpose: This function will cause the processor to, quite literally, explode. In the future,
         * this function may be modified to either cause the part to explode or go into a "damaged" state
         * with the help of a dice roll. This is to help randomize the possible damage from pointing the
         * telescope at the sun.
         * ************************************************************************************************/
        public void Die()
        {
            part.explode();
        }

        /* ************************************************************************************************
         * Function Name: SetType (Deprecated)
         * Input: None
         * Output: None
         * Purpose: This function will allow an external class to change the Type of processor. This function
         * is deprecated.
         * ************************************************************************************************/
        private void SetType(string Type)
        {
            this.Type = Type;
        }

        /* ************************************************************************************************
         * Function Name: ApplyFilter
         * Input: An input texture to be modified, and a string specifying which modification.
         * Output: A modified texture
         * Purpose: This function will return a modified form of the input texture. Possible filters are
         * gray scale, infrared, etc... This function can be overridden, and the string input "Filter"
         * may become deprecated in the future. If not overriden by a sub class, no filter will be 
         * applied.
         * ************************************************************************************************/
        public virtual Texture2D ApplyFilter(string Filter, Texture2D InputTexture)
        {
            return InputTexture;
        }

        /* ************************************************************************************************
         * Function Name: DoScience
         * Input: Position of the target, whether or not we're dealing with the FungEye or CactEye optics,
         *          the current field of view, and a screenshot.
         * Output: None
         * Purpose: This function will generate a science report based on the input parameters. This is a
         * function prototype, and must be implemented by a sub class. This also means that this
         * function's behavoir will change based on what processor is the active processor on the telescope.
         * Please see the individual definitions in sub classes for further details.
         * ************************************************************************************************/
        public abstract string DoScience(Vector3 TargetPosition, bool IsSmallOptics, float FOV, Texture2D Screenshot);

        /* ************************************************************************************************
         * Function Name: OnUpdate
         * Input: None
         * Output: None
         * Purpose: This function will run once every frame. It is used for some event handling, and for
         * producing an electric drain for the craft. 
         * ************************************************************************************************/
        public override void OnUpdate()
        {

            if (Active)
            {
                double PowerDemand = part.RequestResource("ElectricCharge", consumeRate * TimeWarp.deltaTime);
                //Debug.Log("CactEye 2: PowerDemand: " + PowerDemand.ToString());

                //This may be a little pushing it, but the processors are set to be able to run so
                //long as the craft provides at least 15% of their power demand. At power supplies
                //less than 15%, the processor will undergo an ungraceful shutdown. This works so
                //incredibly well that I'm very pleased with the results.
                if (PowerDemand < 0.15 * consumeRate * TimeWarp.deltaTime)
                {
                    Active = false;
                }
            }
        }

        /* ************************************************************************************************
         * Function Name: ActivateProcessor
         * Input: None
         * Output: None
         * Purpose: This function will allow other classes to activate the processor. 
         * ************************************************************************************************/
        public void ActivateProcessor()
        {
            Active = true;
            //CorrectLightDirection();
            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Processor activated! " + Active.ToString());
            }
        }

        /* ************************************************************************************************
         * Function Name: DeactivateProcessor
         * Input: None
         * Output: None
         * Purpose: This function will allow other classes to deactivate the processor. 
         * ************************************************************************************************/
        public virtual void DeactivateProcessor()
        {
            Active = false;
            //RevertLightDirection();
            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Processor deactivated!");
            }
        }

        /* ************************************************************************************************
         * Function Name: GetInfo
         * Input: None
         * Output: Information that is displayed through the details pane in the KSP editor.
         * Purpose: This function will generate the report that you see in the mouse over in the KSP 
         * editor.
         * ************************************************************************************************/
        public override string GetInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tech Type: " + Type);

            sb.AppendLine("Possible Science Gain: " + (maxScience * 100) + "%");
            if (maxScience < 1)
                sb.AppendLine("- More advanced processors can gain more science. You can upgrate later with a service mission!");

            sb.AppendLine();

            sb.AppendLine("Magnification: " + string.Format("{0:n0}", 128 / MinimumFOV) + "x");

            sb.AppendLine();

            sb.AppendLine("<color=#99ff00ff>Requires:</color>");
            sb.AppendLine("- ElectricCharge: " + string.Format("{0:0.0##}", consumeRate) + "/sec");

            sb.AppendLine();

            sb.AppendLine("<color=#ffa500ff>Do not point directly at sun while activated!</color>");

            return sb.ToString();
        }

        private void CorrectLightDirection()
        {

            //if (FlightGlobals.activeTarget.GetType() == typeof(CelestialBody)
            //    && FlightGlobals.activeTarget != FlightGlobals.getMainBody())
            //{

            Light SunReference = GameObject.Find("Sun").GetComponent<Light>();

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: SunReference: " + SunReference.type.ToString());
            }

            Sun.Instance.sunDirection = FlightGlobals.fetch.VesselTarget.GetTransform().position - FlightGlobals.Bodies[0].position;

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: OriginalSunDirection: " + OriginalSunDirection.ToString());
                Debug.Log("CactEye 2: sunDirection: " + Sun.Instance.sunDirection.ToString());
            }

            //}
        }

        private void RevertLightDirection()
        {
            //if (OriginalSunDirection != null)
            //{
                Sun.Instance.sunDirection = OriginalSunDirection;
            //}
        }

        #region Right Click Menu Options

        #endregion

        #region Science Helper Functions

        /* ************************************************************************************************
         * Function Name: _onPageDiscard
         * Input: The current science experiment
         * Output: None
         * Purpose: This is a coroutine that is called when the "discard data" button in a Science report
         * dialog box is clicked. This resets the current science experiment.
         * ************************************************************************************************/
        private void _onPageDiscard(ScienceData Data)
        {
            StoredData.Remove(Data);
            ResetExperimentGUI();
            return;
        }

        /* ************************************************************************************************
         * Function Name: _onPageKeep
         * Input: The current science experiment
         * Output: None
         * Purpose: This is a coroutine that is called when the "keep data" button in a Science report
         * dialog box is clicked. This stores the current science experiment in the part.
         * ************************************************************************************************/
        private void _onPageKeep(ScienceData Data)
        {
            StoredData.Add(Data);
            ResetExperimentGUI();
            return;
        }

        /* ************************************************************************************************
         * Function Name: _onPageTransmit
         * Input: The current science experiment
         * Output: None
         * Purpose: This is a coroutine that is called when the "transmit data" button in a Science report
         * dialog box is clicked. This will selec the first available antennea to transmit the current
         * science experiment back to the Research & Development center.
         * ************************************************************************************************/
        private void _onPageTransmit(ScienceData Data)
        {
            //Grab list of available antenneas
            List<IScienceDataTransmitter> AvailableTransmitters = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();

            if (AvailableTransmitters.Count() > 0)
            {
                AvailableTransmitters.First().TransmitData(new List<ScienceData>{ Data });
            }

            ResetExperimentGUI();
        }

        /* ************************************************************************************************
         * Function Name: _onPageSendToLab
         * Input: The current science experiment
         * Output: None
         * Purpose: This is a coroutine that is called when the "send to lab data" button in a Science report
         * dialog box is clicked. Right now, this does absolutely nothing. This may change in the future,
         * however, as the radio telescope feature is implemented.
         * ************************************************************************************************/
        private void _onPageSendToLab(ScienceData Data)
        {
            //List<ModuleScienceLab> AvailableLabs = vessel.FindPartModulesImplementing<ModuleScienceLab>();

            //if (AvailableLabs.Count() > 0)
            //{
            //    AvailableLabs.First().ProcessData(Data, ReviewData2);
            //}

            ResetExperimentGUI();
            return;
        }

        /* ************************************************************************************************
         * Function Name: ReviewData
         * Input: The current science experiment and a pretty screenshot
         * Output: None
         * Purpose: This is called when a science report is generated, or when the player is revisting
         * stored science experiments. This simply calls a coroutine, so as to allow execution without 
         * dropping the frame rate.
         * ************************************************************************************************/
        public void ReviewData(ScienceData Data, Texture2D Screenshot) 
        {
            StartCoroutine(ReviewDataCoroutine(Data, Screenshot));
        }

        /* ************************************************************************************************
         * Function Name: ReviewDataCoroutine
         * Input: The current science experiment and a pretty screenshot
         * Output: None
         * Purpose: Called as a coroutine; the game will not wait for this function to finish executing
         * before the end of a frame. This displays the science report dialog box, of which the player can
         * interact with.
         * ************************************************************************************************/
        public System.Collections.IEnumerator ReviewDataCoroutine(ScienceData Data, Texture2D Screenshot)
        {
            yield return new WaitForEndOfFrame();

            //GUIStyle ProgressStyle;
            //GUIStyle ScienceStyle;
            
            ExperimentResultDialogPage page = new ExperimentResultDialogPage
                (
                FlightGlobals.ActiveVessel.rootPart,    //hosting part
                Data,                                   //Science data
                Data.transmitValue,                     //scalar for transmitting the data
                Data.labBoost,                          //scalar for lab bonuses
                false,                                  //bool for show transmit warning
                "",                                     //string for transmit warning
                false,                                  //show the reset button
                false,                                  //show the lab option
                new Callback<ScienceData>(_onPageDiscard), 
                new Callback<ScienceData>(_onPageKeep), 
                new Callback<ScienceData>(_onPageTransmit), 
                new Callback<ScienceData>(_onPageSendToLab)
                );

            //page.scienceValue = 0f;
            ExperimentsResultDialog ScienceDialog = ExperimentsResultDialog.DisplayResult(page);

            //Store the old dialog gui information
            ProgressStyle = ScienceDialog.guiSkin.customStyles.Where(n => n.name == "progressBarFill2").First();
            GUIStyle style = ScienceDialog.guiSkin.box;
            StyleDefault = style.normal;
            SkinStored = ScienceDialog.guiSkin;

            ////Lets put a pretty picture on the science dialog.
            ScienceStyle = ScienceDialog.guiSkin.box;
            ScienceStyle.normal.background = Screenshot;

            ScienceDialog.guiSkin.window.fixedWidth = 587f;
            ScienceStyle.fixedWidth = 512f;
            ScienceStyle.fixedHeight = 288f;

            
        }


        private void ResetExperimentGUI()
        {
            //print("Resetting GUI...");
            if (SkinStored != null)
            {
                SkinStored.box.normal = StyleDefault;
                SkinStored.box.normal.background = GameDatabase.Instance.GetTexture("CactEye/Icons/ExperimentGUIBackground", false);
                SkinStored.box.fixedWidth = 0f;
                SkinStored.box.fixedHeight = 0f;
                SkinStored.window.fixedWidth = 400f;
            }
            else
            {
                Debug.Log("CactEye 2: Logical Error 3: SkinStored is null!");
            }

            ScienceStyle.fixedHeight = 0f;
            ScienceStyle.fixedWidth = 0f;
        }

        #endregion
    }
}
