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
        private List<CactEyeGyro> ReactionWheels = new List<CactEyeGyro>();
        private List<float> ReactionWheelPitchTorques = new List<float>();
        private List<float> ReactionWheelYawTorques = new List<float>();
        private List<float> ReactionWheelRollTorques = new List<float>();


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
                RenderingManager.AddToPostDrawQueue(3, new Callback(DrawGUI));

                //Moved to here from the constructor; this should get a new list of gyros
                //every time the player enables the menu to account for part changes due to
                //docking/undocking operations.
                try
                {
                    //Grab Reaction Wheels
                    GetReactionWheels();
                    GetProcessors();
                }
                catch (Exception E)
                {
                    Debug.Log("CactEye 2: Exception 3: Was not able to get a list of Reaction Wheels.");
                    Debug.Log(E.ToString());
                }
            }
 
            else
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawGUI));
            }
            IsGUIVisible = !IsGUIVisible;
        }

        private void MainGUI(int WindowID)
        {
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

            //Draw Processor controls in bottom center of display, with observation and screenshot buttons in center.
            DrawProcessorControls();
            DrawTargetPointer();

            if (ActiveProcessor)
            {
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
                GUILayout.EndHorizontal();

                //Zoom Slider Controls.
                GUILayout.BeginHorizontal();
                FieldOfView = GUILayout.HorizontalSlider(FieldOfView, 0f, 1f);
                CameraModule.FieldOfView = 0.5f * Mathf.Pow(4f - FieldOfView * (4f - Mathf.Pow(0.1f, (1f / 3f))), 3);
                GUILayout.EndHorizontal();
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
            WindowPosition = GUILayout.Window(WindowId, WindowPosition, MainGUI, WindowTitle);
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
            return FieldOfView;
        }

        //Don't touch; it works.
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

        private void DrawProcessorControls()
        {

            //Draw save icon
            if (ActiveProcessor && ActiveProcessor.Type.Contains("Wide Field"))
            {
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) + 20), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), SaveScreenshotTexture))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                }
            }

            //Draw gather science icon
            //Atom6 icon from Freepik
            //<div>Icons made by Freepik from <a href="http://www.flaticon.com" title="Flaticon">www.flaticon.com</a>         is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0">CC BY 3.0</a></div>
            if (ActiveProcessor && HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
            {
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) - 20), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Atom6Icon))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                }
            }

            //Previous/Next buttons
            if (Processors.Count<CactEyeProcessor>() > 1)
            {
                //Previous button
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) - 72), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Back9Icon))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                }

                //Next Button
                if (GUI.Button(new Rect(ScopeRect.xMin + ((0.5f * ScopeRect.width) + 72), ScopeRect.yMin + (ScopeRect.height - 48f), 32, 32), Forward9Icon))
                {
                    //DisplayText("Saved screenshot to " + opticsModule.GetTex(true, targetName));
                }
            }
        }

        //Refactored 11/3/2014
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
            }
        }

        private void SetTorgue()
        {

            for (int i = 0; i < ReactionWheels.Count(); i++)
            {
                ReactionWheels[i].GyroSensitivity = GyroSensitivity;
                //ReactionWheels[i].PitchTorque = ReactionWheelPitchTorques[i] * GyroSensitivity;
                //ReactionWheels[i].YawTorque = ReactionWheelYawTorques[i] * GyroSensitivity;
                //ReactionWheels[i].RollTorque = ReactionWheelRollTorques[i] * GyroSensitivity;
            }
        }
    }
}
