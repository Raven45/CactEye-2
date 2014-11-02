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
            //CameraModule.CameraTransform = Position;

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
            //Top right hand corner button that exits the window.
            if (GUI.Button(new Rect(WindowPosition.width - 18, 2, 16, 16), ""))
            {
                Toggle();
            }

            //What you see looking through the telescope.
            ScopeRect = GUILayoutUtility.GetRect(Screen.width * 0.4f, Screen.width*0.4f);
            Texture2D ScopeScreen = CameraModule.UpdateTexture();
            GUI.DrawTexture(ScopeRect, ScopeScreen);

            //Zoom Feedback Label.
            string LabelZoom = "Zoom/Magnification: x";
            LabelZoom += string.Format("{0:####0.0}", 64 / FieldOfView);
            GUILayout.BeginHorizontal();
            GUI.skin.GetStyle("Label").alignment = TextAnchor.UpperCenter;
            GUILayout.Label(LabelZoom);
            GUILayout.EndHorizontal();

            //Zoom Slider Controls.
            GUILayout.BeginHorizontal();
            FieldOfView = GUILayout.HorizontalSlider(FieldOfView, 1f, 0f);
            CameraModule.FieldOfView = 0.5f * Mathf.Pow(4f - FieldOfView * (4f - Mathf.Pow(0.1f, (1f / 3f))), 3);
            GUILayout.EndHorizontal();
        }

        private void DrawGUI()
        {
            WindowPosition = GUILayout.Window(WindowId, WindowPosition, MainGUI, WindowTitle);
        }

        public void UpdatePosition(Transform Position)
        {
            //this.Position = Position;
            CameraModule.UpdatePosition(Position);
        }
    }
}
