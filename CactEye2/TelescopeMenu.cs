using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    class TelescopeMenu: MonoBehaviour
    {
        
        //Position and size of the window
        private Rect WindowPosition;

        //Unique ID and window title for the GUI.
        private int WindowId;
        private string WindowTitle;

        //Flag that detects if the GUI is enabled or not.
        private bool IsGUIVisible = false;

        //Gui is 80% of screen resolution.
        private float ScreenToGUIRatio = 0.8f;

        //Field of view of the scope camera.
        private float FieldOfView = 0f;
        private float GyroSensitivity = 1f;

        //Control variable for enabling the Gyro.
        private bool GyroEnabled = false;

        private Rect ScopeRect;

        private CactEyeCamera CameraModule;

        //Textures
        private Texture2D PreviewTexture = null;
        private Texture2D CrosshairTexture = null;
        private Texture2D TargetPointerTexture = null;
        private Texture2D SaveScreenshotTexture = null;
        private Texture2D Atom6Icon = null;
        private Texture2D Back9Icon = null;
        private Texture2D Forward9Icon = null;

        //private ModuleReactionWheel[] ReactionWheels;
        private List<CactEyeProcessor> Processors = new List<CactEyeProcessor>();
        private CactEyeProcessor ActiveProcessor;
        private int CurrentProcessorIndex = 0;
        private List<CactEyeGyro> ReactionWheels = new List<CactEyeGyro>();
        private List<float> ReactionWheelPitchTorques = new List<float>();
        private List<float> ReactionWheelYawTorques = new List<float>();
        private List<float> ReactionWheelRollTorques = new List<float>();

        //Status message for player
        private string Notification = "";
        static private double timer = 6f;
        private double storedTime = 0f;

        //Check for pause menu
        //private bool GameIsPaused = false;


        public TelescopeMenu(Transform Position)
        {

            //unique id for the gui window.
            this.WindowTitle = "CactEye Telescope Control System";
            this.WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);

            //Grabbin' textures
            PreviewTexture = GameDatabase.Instance.GetTexture("CactEye/Icons/preview", false);
            PreviewTexture.filterMode = FilterMode.Point;
            CrosshairTexture = GameDatabase.Instance.GetTexture("CactEye/Icons/crosshair", false);
            TargetPointerTexture = GameDatabase.Instance.GetTexture("CactEye/Icons/target", false);
            SaveScreenshotTexture = GameDatabase.Instance.GetTexture("CactEye/Icons/save", false);
            Atom6Icon = GameDatabase.Instance.GetTexture("CactEye/Icons/atom6", false);
            Back9Icon = GameDatabase.Instance.GetTexture("CactEye/Icons/back19", false);
            Forward9Icon = GameDatabase.Instance.GetTexture("CactEye/Icons/forward19", false);

            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = Screen.width * ScreenToGUIRatio;
            float WindowHeight = Screen.height * ScreenToGUIRatio;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);
            
            //Attempt to create the Telescope camera object.
            try
            {
                CameraModule = new CactEyeCamera(Position);
            }
            catch (Exception E)
            {
                Debug.Log("CactEye 2: Exception 2: Was not able to create the camera object.");
                Debug.Log(E.ToString());
            }
        }

        //Might take a look at using lazy initialization for enabling/disabling the menu object
        public void Toggle()
        {
            if (!IsGUIVisible)
            {
                //Moved to here from the constructor; this should get a new list of gyros
                //every time the player enables the menu to account for part changes due to
                //docking/undocking operations.
                try
                {
                    //Grab Reaction Wheels
                    GetReactionWheels();
                    GetProcessors();

                    //if (ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                    //{
                    //    ActiveProcessor.ActivateProcessor();
                    //}

                    ActiveProcessor.ActivateProcessor();
                }
                catch (Exception E)
                {
                    Debug.Log("CactEye 2: Exception 3: Was not able to get a list of Reaction Wheels or Processors.");
                    Debug.Log(E.ToString());
                }

                RenderingManager.AddToPostDrawQueue(3, new Callback(DrawGUI));
            }
 
            else
            {
                if (ActiveProcessor != null)
                {
                    if (ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                    {
                        ActiveProcessor.DeactivateProcessor();
                    }
                    ActiveProcessor = null;
                }

                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawGUI));
            }
            IsGUIVisible = !IsGUIVisible;
        }

        private void MainGUI(int WindowID)
        {
            timer += Planetarium.GetUniversalTime() - storedTime;
            storedTime = Planetarium.GetUniversalTime();

            //Top right hand corner button that exits the window.
            if (GUI.Button(new Rect(WindowPosition.width - 18, 2, 16, 16), ""))
            {
                Toggle();
            }

            //What you see looking through the telescope.
            ScopeRect = GUILayoutUtility.GetRect(Screen.width * 0.4f, Screen.width*0.4f);
            Texture2D ScopeScreen = CameraModule.UpdateTexture(ActiveProcessor);
            GUI.DrawTexture(ScopeRect, ScopeScreen);

            //Draw the preview texture
            GUI.DrawTexture(new Rect(ScopeRect.xMin, ScopeRect.yMax - 32f, 128f, 32f), PreviewTexture);
            //Draw the crosshair texture
            GUI.DrawTexture(new Rect(ScopeRect.xMin + (0.5f * ScopeRect.width) - 64, ScopeRect.yMin + (0.5f * ScopeRect.height) - 64, 128, 128), CrosshairTexture);
            //Draw the notification label
            if (timer > 5f)
            {
                Notification = "";
            }
            GUI.Label(new Rect(ScopeRect.xMin + 16, ScopeRect.yMin + 16, 600, 32), new GUIContent(Notification)); 

            //Draw Processor controls in bottom center of display, with observation and screenshot buttons in center.
            DrawProcessorControls();
            DrawTargetPointer();

            if (ActiveProcessor)
            {
                //Close window down if we run out of power
                if (!ActiveProcessor.IsActive())
                {
                //    Toggle();
                //    ScreenMessages.PostScreenMessage("Image processor is out of power. Please restore power to telescope.", 6, ScreenMessageStyle.UPPER_CENTER);
                    ActiveProcessor = null;
                    Notification = "Image Processor is out of power. Please restore power to telescope";
                    timer = 0f;
                }

                //Zoom Feedback Label.
                string LabelZoom = "Zoom/Magnification: x";
                if (CameraModule.FieldOfView > 0.0064)
                {
                    LabelZoom += string.Format("{0:####0.0}", 64 / CameraModule.FieldOfView);
                }
                else
                {
                    LabelZoom += string.Format("{0:0.00E+0}", (64 / CameraModule.FieldOfView));
                }
                GUILayout.BeginHorizontal();
                GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
                GUILayout.Label(LabelZoom);

                //Active Processor and status Label
                GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
                GUILayout.Label("Active Processor: " + ActiveProcessor.GetProcessorType());
                GUILayout.EndHorizontal();

                //Zoom Slider Controls.
                GUILayout.BeginHorizontal();
                FieldOfView = GUILayout.HorizontalSlider(FieldOfView, 0f, 1f);
                CameraModule.FieldOfView = 0.5f * Mathf.Pow(4f - FieldOfView * (4f - Mathf.Pow(ActiveProcessor.GetMinimumFOV(), (1f / 3f))), 3);
                GUILayout.EndHorizontal();

                //Log spam
                //Debug.Log("CactEye 2: MinimumFOV = " + ActiveProcessor.GetMinimumFOV().ToString());
            }

            else
            {
                GUILayout.BeginHorizontal();
                GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
                GUILayout.Label("Processor not installed; optics module cannot function without an image processor.");
                GUILayout.EndHorizontal();
            }

            //Gyro GUI. Active only if the craft has an active gyro
            if (GyroEnabled)
            {
                //Gyro Slider Label
                GUILayout.BeginHorizontal();
                GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperLeft;
                GUILayout.Label("Gyro Sensitivity:  " + GyroSensitivity.ToString("P") + " + minimum gyroscopic torgue.", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                //Gyro Slider Controls.
                GUILayout.BeginHorizontal();
                GyroSensitivity = GUILayout.HorizontalSlider(GyroSensitivity, 0f, 1f, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                SetTorgue();
            }

            //Make the window draggable by the top bar only.
            GUI.DragWindow(new Rect(0, 0, WindowPosition.width, 16));
        }

        private void DrawGUI()
        {

            try
            {
                if (!PauseMenu.isOpen && !FlightResultsDialog.isDisplaying && !MapView.MapIsEnabled)
                {
                    WindowPosition = GUILayout.Window(WindowId, WindowPosition, MainGUI, WindowTitle);
                }
            }
            //Ignore the pesky error and assume game is not paused.
            catch
            {
                WindowPosition = GUILayout.Window(WindowId, WindowPosition, MainGUI, WindowTitle);
            }
        }

        public void UpdatePosition(Transform Position)
        {
            CameraModule.UpdatePosition(Position);
        }

        public void ToggleGyro()
        {
            GyroEnabled = !GyroEnabled;
        }

        public Vector3 GetTargetPos(Vector3 worldPos, float width)
        {
            //Camera c = cameras.Find(n => n.name.Contains("00"));
            Camera c = CameraModule.GetCamera(2);
            Vector3 vec = c.WorldToScreenPoint(worldPos);

            if (Vector3.Dot(CameraModule.CameraTransform.forward, worldPos) > 0)
            {
                if (vec.x > 0 && vec.y > 0 && vec.x < c.pixelWidth && vec.y < c.pixelHeight)
                {
                    vec.y = c.pixelHeight - vec.y;
                    vec *= (width / c.pixelWidth);
                    return vec;
                }
            }

            return new Vector3(-1, -1, 0);
        }

        public float GetGyroSensitivty()
        {
            return GyroSensitivity;
        }

        public bool IsMenuEnabled()
        {
            return IsGUIVisible;
        }

        public float GetFOV()
        {
            return CameraModule.FieldOfView;
        }

        /* ************************************************************************************************
         * Function Name: DrawTargetPointer
         * Input: None
         * Output: None
         * Purpose: This function will draw the pink target recticle in the scope's view. This works
         * rather well, so don't touch this unless it's absolutely neccesary, as there's a lot of moving
         * parts here.
         * ************************************************************************************************/
        private void DrawTargetPointer()
        {

            if (FlightGlobals.fetch.VesselTarget != null)
            {
                string targetName = FlightGlobals.fetch.VesselTarget.GetName();
                Vector2 vec = GetTargetPos(FlightGlobals.fetch.VesselTarget.GetTransform().position, ScopeRect.width);

                if (vec.x > 16 && vec.y > 16 && vec.x < ScopeRect.width - 16 && vec.y < ScopeRect.height - 16)
                {
                    GUI.DrawTexture(new Rect(vec.x + ScopeRect.xMin - 16, vec.y + ScopeRect.yMin - 16, 32, 32), TargetPointerTexture);
                    Vector2 size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(targetName));
                    if (vec.x > 0.5 * size.x && vec.x < ScopeRect.width - (0.5 * size.x) && vec.y < ScopeRect.height - 16 - size.y)
                    {
                        GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
                        GUI.Label(new Rect(vec.x + ScopeRect.xMin - (0.5f * size.x), vec.y + ScopeRect.yMin + 20, size.x, size.y), targetName);
                    }
                }
            }
        }

        /* ************************************************************************************************
         * Function Name: DrawProcessorControls
         * Input: None
         * Output: None
         * Purpose: This function will draw the four main processor control objects: the screenshot
         * button, the science button, and the next/previous processor buttons.
         * The screenshot button will only display and be available if the telescope has a valid
         * processor installed on the scope.
         * The science button will only appear if a target is selected, if there is a valid processor
         * installed, and if the game is not a sandbox game. It will generate a science report based
         * on the selected target.
         * The next/previous buttons will only appear if the scope has more than one processor installed,
         * and will allow the player to cycle through the different processors.
         * ************************************************************************************************/
        private void DrawProcessorControls()
        {
            //if (!ActiveProcessor.IsActive())
            //{
            //    //Craft is out of power.
            //    Notification = "Image processor is out of power; shutting down processor.";
            //    timer = 0f;
            //    Processors.Remove(ActiveProcessor);
            //}

            //Draw save icon
            if (FlightGlobals.fetch.VesselTarget != null && ActiveProcessor && ActiveProcessor.GetProcessorType().Contains("Wide Field"))
            {
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) + 20), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), SaveScreenshotTexture))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                    Notification = " Screenshot saved to " + WriteTextureToDrive(CameraModule.TakeScreenshot(ActiveProcessor));
                    timer = 0f;
                }
            }

            //Draw gather science icon
            //Atom6 icon from Freepik
            //<div>Icons made by Freepik from <a href="http://www.flaticon.com" title="Flaticon">www.flaticon.com</a>         is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0">CC BY 3.0</a></div>
            if (FlightGlobals.fetch.VesselTarget != null && ActiveProcessor && HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
            {
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) - 20), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Atom6Icon))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                    //ActiveProcessor.GenerateScienceReport(TakeScreenshot(ActiveProcessor.GetType()));
                    try
                    {
                        Notification = ActiveProcessor.DoScience(GetTargetPos(FlightGlobals.fetch.VesselTarget.GetTransform().position, 500f), false, CameraModule.FieldOfView, CameraModule.TakeScreenshot(ActiveProcessor));
                    }
                    catch (Exception e)
                    {
                        Notification = "An error occured. Please post that you're having this error on the official CactEye 2 thread on the Kerbal Forums.";
                        Debug.Log("CactEye 2: Exception 4: An error occured producing a science report!");
                        Debug.Log(e.ToString());
                    }

                    timer = 0f;
                }
            }

            //Got an off-by-one error in the list somewhere
            //Previous/Next buttons
            if (Processors.Count<CactEyeProcessor>() > 1)
            {
                //Previous button
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) - 72), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Back9Icon))
                {
                    ActiveProcessor.Active = false;
                    ActiveProcessor = GetPrevious(Processors, ActiveProcessor);
                    ActiveProcessor.Active = true;
                }

                //Next Button
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) + 72), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Forward9Icon))
                {
                    ActiveProcessor.Active = false;
                    ActiveProcessor = GetNext(Processors, ActiveProcessor);
                    ActiveProcessor.Active = true;
                }
            }
        }

        /* ************************************************************************************************
         * Function Name: GetReactionWheels
         * Input: none
         * Output: None
         * Purpose: This function will grab a list of gyroscopes installed on the scope's craft. The name
         * is leftover from a previous functionality, of which the function use to return a list of all
         * reactionwheels, including command modules and gyroscopes. 
         * 
         * This was heavily refactored on 11/3/2014 by Raven.
         * ************************************************************************************************/
        private void GetReactionWheels()
        {
            ReactionWheels.Clear();

            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                CactEyeGyro mrw = p.GetComponent<CactEyeGyro>();
                if (mrw != null)
                {
                    if (!ReactionWheels.Contains(mrw))
                    {
                        ReactionWheels.Add(mrw);
                        GyroSensitivity = mrw.GyroSensitivity;
                    }
                }
            }

            Debug.Log("CactEye 2: Found " + ReactionWheels.Count().ToString() + " Gyro units.");

            if (ReactionWheels.Count<CactEyeGyro>() > 0)
            {
                GyroEnabled = true;
                ReactionWheelPitchTorques = ReactionWheels.Select(CactEyeGyro => CactEyeGyro.PitchTorque).ToList();
                ReactionWheelYawTorques = ReactionWheels.Select(CactEyeGyro => CactEyeGyro.YawTorque).ToList();
                ReactionWheelRollTorques = ReactionWheels.Select(CactEyeGyro => CactEyeGyro.RollTorque).ToList();
            }
            else
            {
                GyroEnabled = false;
            }
        }

        /* ************************************************************************************************
         * Function Name: GetProcessors
         * Input: None
         * Output: None
         * Purpose: This function will generate a list of image processors installed on the telescope
         * craft.
         * ************************************************************************************************/
        private void GetProcessors()
        {
            Processors.Clear();

            foreach (Part p in FlightGlobals.ActiveVessel.Parts)
            {
                CactEyeProcessor cpu = p.GetComponent<CactEyeProcessor>();
                if (cpu != null)
                {
                    if (!Processors.Contains(cpu))
                    {
                        Processors.Add(cpu);
                    }
                }
            }

            Debug.Log("CactEye 2: Found " + Processors.Count().ToString() + " Processors.");

            if (Processors.Count<CactEyeProcessor>() > 0)
            {
                ActiveProcessor = Processors.First<CactEyeProcessor>();
                CurrentProcessorIndex = 0;

                //if (ActiveProcessor.GetProcessorType().Contains("Wide Field"))
                //{
                //    ActiveProcessor.Active = true;
                //}
            }
        }

        /* ************************************************************************************************
         * Function Name: SetTorgue
         * Input: None
         * Output: None
         * Purpose: This function will modify the torgue rating of all gyroscopes installed on the telescope
         * craft. This is tied directly with the gryoscope sensitivity control slider.
         * ************************************************************************************************/
        private void SetTorgue()
        {

            for (int i = 0; i < ReactionWheels.Count(); i++)
            {
                ReactionWheels[i].GyroSensitivity = GyroSensitivity;
            }
        }


        private CactEyeProcessor GetNext(IEnumerable<CactEyeProcessor> list, CactEyeProcessor current)
        {
            try
            {
                //return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
                //lastAgentIDAarhus = agents[ index == -1 ? 0 : index % ( agents.Count - 1 ) ];
                if ((CurrentProcessorIndex + 1) < Processors.Count)
                {
                    CurrentProcessorIndex++;
                }
                else
                {
                    CurrentProcessorIndex = 0;
                }

                //return Processors[CurrentProcessorIndex == -1 ? 0 : CurrentProcessorIndex % (Processors.Count - 1)];
                Debug.Log("CactEye 2: CurrentProcessorIndex: " + CurrentProcessorIndex.ToString());
                return Processors[CurrentProcessorIndex];
            }
            catch (Exception e)
            {
                Debug.Log("CactEye 2: Exception #: Was not able to find the next processor, even though there is one.");
                Debug.Log(e.ToString());

                return Processors.FirstOrDefault();
            }
        }

        private CactEyeProcessor GetPrevious(IEnumerable<CactEyeProcessor> list, CactEyeProcessor current)
        {
            try
            {
                //return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
                //lastAgentIDAarhus = agents[ index == -1 ? 0 : index % ( agents.Count - 1 ) ];
                if (CurrentProcessorIndex == 0)
                {
                    CurrentProcessorIndex = Processors.Count - 1;
                }
                else
                {
                    CurrentProcessorIndex--;
                }

                //return Processors[CurrentProcessorIndex == -1 ? 0 : CurrentProcessorIndex % (Processors.Count - 1)];
                Debug.Log("CactEye 2: CurrentProcessorIndex: " + CurrentProcessorIndex.ToString());
                return Processors[CurrentProcessorIndex];
            }
            catch (Exception e)
            {
                Debug.Log("CactEye 2: Exception #: Was not able to find the next processor, even though there is one.");
                Debug.Log(e.ToString());

                return Processors.FirstOrDefault();
            }
        }

        /* ************************************************************************************************
         * Function Name: WriteTextureToDrive
         * Input: The texture object that will be written to the hard drive.
         * Output: None
         * Purpose: This function will take an input texture and then convert it to a png file in the 
         * CactEye subfolder of the Screenshot folder.
         * This currently has some bugs.
         * If Linux users complain about screenshots not saving to the disk, then this is the first place
         * to look.
         * ************************************************************************************************/
        private string WriteTextureToDrive(Texture2D Input)
        {
            byte[] Bytes = Input.EncodeToPNG();
            string ScreeshotFolderPath = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "Screenshots/CactEye/";
            //string TargetName = FlightGlobals.activeTarget.ToString();
            string TargetName = FlightGlobals.fetch.VesselTarget.GetName().ToString();
            string ScreenshotFilename = "";

            

            //Create CactEye screenshot folder if it doesn't exist
            if (!System.IO.Directory.Exists(ScreeshotFolderPath))
            {
                System.IO.Directory.CreateDirectory(ScreeshotFolderPath);
            }

            ScreenshotFilename = TargetName + CactEyeAPI.Time() + ".png";
            System.IO.File.WriteAllBytes(ScreeshotFolderPath + ScreenshotFilename, Bytes);
            return ScreenshotFilename;
        }
    }
}
