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
        private float fov = 0f;

        private Rect ScopeRect;
        private Transform Position;

        private CactEyeCamera CameraModule;
        

        public TelescopeMenu(Transform Position)
        {
            this.Position = Position;

            //unique id for the gui window.
            this.WindowTitle = "CactEye Telescope Control System";
            this.WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);

            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = Screen.width * ScreenToGUIRatio;
            float WindowHeight = Screen.height * ScreenToGUIRatio;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);
            //ScopeRect = new Rect(0, 0, Screen.width * 0.4f, Screen.height * 0.4f);
            
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

        public void Toggle()
        {
            if (!IsGUIVisible)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(DrawGUI));
            }
 
            else
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawGUI));
            }
            IsGUIVisible = !IsGUIVisible;

            
        }

        private void MainGUI(int WindowID)
        {



            ScopeRect = GUILayoutUtility.GetRect(Screen.width * 0.4f, Screen.width*0.4f);
            Texture2D ScopeScreen = CameraModule.UpdateTexture();
            GUI.DrawTexture(ScopeRect, ScopeScreen);

            //Zoom Feedback
            string LabelZoom = "Zoom/Magnification: x";
            LabelZoom += string.Format("{0:####0.0}", 64 / FieldOfView);
            GUILayout.BeginHorizontal();
            GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
            GUILayout.Label(LabelZoom);
            GUILayout.EndHorizontal();

            //Zoom Slider
            GUILayout.BeginHorizontal();
            FieldOfView = GUILayout.HorizontalSlider(FieldOfView, 0.00001f, 6f);
            GUILayout.EndHorizontal();
        }

        private void DrawGUI()
        {
            WindowPosition = GUILayout.Window(WindowId, WindowPosition, MainGUI, WindowTitle);
        }

        public void UpdatePosition(Transform Position)
        {
            this.Position = Position;
            CameraModule.CameraTransform = Position;
            CameraModule.FieldOfView = FieldOfView;
        }
    }
}
