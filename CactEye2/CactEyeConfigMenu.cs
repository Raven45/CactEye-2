using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class CactEyeConfigMenu: MonoBehaviour
    {
        //Position and size of the window
        private Rect WindowPosition;

        //Unique ID and window title for the GUI.
        private int WindowId;
        private string WindowTitle;

        //Flag that detects if the GUI is enabled or not.
        private bool IsGUIVisible = false;

        private static ApplicationLauncherButton appLauncherButton = null;
        private Texture2D Icon = null;

        private bool SunDamage;
        private bool GyroDecay;

        public CactEyeConfigMenu()
        {

            //unique id for the gui window.
            this.WindowTitle = "CactEye 2 Configuration Menu";
            this.WindowId = WindowTitle.GetHashCode() + new System.Random().Next(65536);

            //Create the window rectangle object
            float StartXPosition = Screen.width * 0.1f;
            float StartYPosition = Screen.height * 0.1f;
            float WindowWidth = 200;
            float WindowHeight = 100;
            WindowPosition = new Rect(StartXPosition, StartYPosition, WindowWidth, WindowHeight);
        }


        public void Awake() 
        {
            if (ApplicationLauncher.Ready)
            {
                appLauncherButton = InitializeApplicationButton();

                if (appLauncherButton != null)
                {
                    appLauncherButton.VisibleInScenes = ApplicationLauncher.AppScenes.SPACECENTER;

                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Debug: Application Launcher Button created!");
                    }
                }
            }
        }

        public void OnDestroy()
        {
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Debug: Application Launcher Button destroyed!");
                }
            }
        }

        ApplicationLauncherButton InitializeApplicationButton()
        {
            ApplicationLauncherButton Button = null;
            Icon = GameDatabase.Instance.GetTexture("CactEye/Icons/CactEyeOptics_scaled", false);

            if (Icon == null)
            {
                Debug.Log("CactEye 2: Logical Error: Was not able to load application launcher icon");
            }

            else
            {
                Button = ApplicationLauncher.Instance.AddModApplication(
                    OnAppLauncherTrue,
                    OnAppLauncherFalse,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    Icon);

                if (Button == null)
                {
                    Debug.Log("CactEye 2: Logical Error: Was not able to add the application launcher button!");
                }
            }


            return Button;
        }

        void OnAppLauncherTrue()
        {
            Toggle();

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Debug: OnAppLauncherTrue() fired!");
            }

        }

        void OnAppLauncherFalse()
        {
            Toggle();

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Debug: OnAppLauncherFalse() fired!");
            }
        }


        public void Toggle()
        {
            if (!IsGUIVisible)
            {
                RenderingManager.AddToPostDrawQueue(3, new Callback(DrawGUI));

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Debug: CactEyeConfigMenu enabled!");
                }
            }
            else
            {
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawGUI));

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Debug: CactEyeConfigMenu disabled!");
                }
            }
            IsGUIVisible = !IsGUIVisible;
        }

        private void MainGUI(int WindowID)
        {

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Debug: CactEyeConfigMenu.MainGUI called!");
            }

            //Top right hand corner button that exits the window.
            if (GUI.Button(new Rect(WindowPosition.width - 18, 2, 16, 16), ""))
            {
                Toggle();
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            CactEyeConfig.DebugMode = GUILayout.Toggle(CactEyeConfig.DebugMode, "Enable Debug Mode.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            SunDamage = GUILayout.Toggle(SunDamage, "Enable Sun Damage to Telescopes.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            SunDamage = GUILayout.Toggle(GyroDecay, "Enable Gyroscope decay over time.");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            //Make the window draggable by the top bar only.
            GUI.DragWindow(new Rect(0, 0, WindowPosition.width, 16));
        }

        private void DrawGUI()
        {
            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Debug: Callback to DrawGUI occurred!");
            }

            WindowPosition = GUILayout.Window(WindowId, WindowPosition, MainGUI, WindowTitle);
        }
    }
}
