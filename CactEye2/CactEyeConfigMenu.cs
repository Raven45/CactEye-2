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

        private bool DebugMode = false;
        private bool SunDamage = false;
        private bool GyroDecay = false;

        /* ************************************************************************************************
         * Function Name: CactEyeConfigMenu
         * Input: N/A
         * Output: N/A
         * Purpose: Default constructor for the configuration menu. This will set several different 
         * initial values for the config menu GUI.
         * ************************************************************************************************/
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

        /* ************************************************************************************************
         * Function Name: Awake
         * Input: N/A
         * Output: N/A
         * Purpose: Awake is a function that Unity looks for when instantiating a MonoBehavior or derived
         * class. Awake should fire at the start of a scene, typically directly after a scene change.
         * ************************************************************************************************/
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

        /* ************************************************************************************************
         * Function Name: OnDestroy
         * Input: N/A
         * Output: N/A
         * Purpose: OnDestroy is a function that Unity look for when destroying a MonoBehavior or derived
         * class. Think of it as a destructor; it's called on the destruction of an object, and in Unity
         * allows the programmer to clean things up. In this case, OnDestroy will remove the application
         * launcher button so we don't get duplicate buttons in the SpaceCenter.
         * ************************************************************************************************/
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

        /* ************************************************************************************************
         * Function Name: InitializeApplicationButton
         * Input: N/A
         * Output: A reference to the newly created button.
         * Purpose: This function will initialize the application launcher button for CactEye, and specify
         * which functions to call when a user clicks the button.
         * ************************************************************************************************/
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

        /* ************************************************************************************************
         * Function Name: OnAppLauncherTrue
         * Input: N/A
         * Output: N/A
         * Purpose: This function is called when a user clicks the application launcher button and the 
         * configuration menu is not being displayed. Essentially it brings up the configuration menu.
         * ************************************************************************************************/
        void OnAppLauncherTrue()
        {
            Toggle();

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Debug: OnAppLauncherTrue() fired!");
            }

        }

        /* ************************************************************************************************
         * Function Name: OnAppLauncherFalse
         * Input: N/A
         * Output: N/A
         * Purpose: This function is called when a user clicks the application launcher button and the 
         * configuration menu is not being displayed. Essentially it hides the configuration menu.
         * ************************************************************************************************/
        void OnAppLauncherFalse()
        {
            Toggle();

            if (CactEyeConfig.DebugMode)
            {
                Debug.Log("CactEye 2: Debug: OnAppLauncherFalse() fired!");
            }
        }


        /* ************************************************************************************************
         * Function Name: Toggle
         * Input: N/A
         * Output: N/A
         * Purpose: This function will show or hide the configuration menu, depending on whether or not 
         * the configuration menu is already up.
         * ************************************************************************************************/
        public void Toggle()
        {
            if (!IsGUIVisible)
            {

                CactEyeConfig.ReadSettings();
                DebugMode = CactEyeConfig.DebugMode;
                SunDamage = CactEyeConfig.SunDamage;
                GyroDecay = CactEyeConfig.GyroDecay;

                RenderingManager.AddToPostDrawQueue(3, new Callback(DrawGUI));

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Debug: CactEyeConfigMenu enabled!");
                }
            }

            else
            {

                CactEyeConfig.DebugMode = DebugMode;
                CactEyeConfig.SunDamage = SunDamage;
                CactEyeConfig.GyroDecay = GyroDecay;
                CactEyeConfig.ApplySettings();

                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawGUI));

                if (CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Debug: CactEyeConfigMenu disabled!");
                }
            }

            IsGUIVisible = !IsGUIVisible;
        }

        /* ************************************************************************************************
         * Function Name: MainGUI
         * Input: N/A
         * Output: N/A
         * Purpose: This function will draw the configuration menu GUI, and define the individual controls
         * on that GUI.
         * ************************************************************************************************/
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
            DebugMode = GUILayout.Toggle(DebugMode, "Enable Debug Mode.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            SunDamage = GUILayout.Toggle(SunDamage, "Enable Sun Damage to Telescopes.");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GyroDecay = GUILayout.Toggle(GyroDecay, "Enable Gyroscope decay over time.");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            //Make the window draggable by the top bar only.
            GUI.DragWindow(new Rect(0, 0, WindowPosition.width, 16));
        }

        /* ************************************************************************************************
         * Function Name: DrawGUI
         * Input: N/A
         * Output: N/A
         * Purpose: This function is called when the Toggle function is called. This will define the 
         * configuration menu window and then call MainGUI.
         * ************************************************************************************************/
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
