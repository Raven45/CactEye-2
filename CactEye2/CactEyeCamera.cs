using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace CactEye2
{
    class CactEyeCamera: MonoBehaviour
    {
        //Camera resolution
        private int CameraWidth = 400;
        private int CameraHeight = 400;

        //Linear transform of the cameras
        public Transform CameraTransform;
        //Field of view of the camera
        public float FieldOfView;

        public bool RotationLock = false;

        //Texture stuff...
        private RenderTexture ScopeRenderTexture;
        private RenderTexture FullResolutionTexture;
        private Texture2D ScopeTexture2D;
        private Texture2D FullTexture2D;

        //I wonder if C# has a map data structure; a map would simplify some things
        private List <Camera>CameraObject;
        private String[] CameraNames =
        {
            "GalaxyCamera",
            "Camera ScaledSpace",
            "Camera 01",
            "Camera 00"
        };

        private Renderer[] skyboxRenderers;
        private ScaledSpaceFader[] scaledSpaceFaders;

        /*
         * Constructor
         * Input: The owning part's transform.
         * Purpose: This constructor will start up the owning part's camera object. The idea behind this
         * was to allow for multiple telescopes on the same craft. 
         */
        public CactEyeCamera(Transform Position)
        {
            this.CameraTransform = Position;
            CameraObject = new List<Camera>();
//            CameraWidth = (int)(Screen.width*0.4f);
//            CameraHeight = (int)(Screen.height*0.4f);

            ScopeRenderTexture = new RenderTexture(CameraWidth, CameraHeight, 24);
            ScopeRenderTexture.Create();

            FullResolutionTexture = new RenderTexture(Screen.width, Screen.height, 24);
            FullResolutionTexture.Create();

            ScopeTexture2D = new Texture2D(CameraWidth, CameraHeight);
            FullTexture2D = new Texture2D(Screen.width, Screen.height);

            foreach (String camName in CameraNames)
            {
                CameraSetup(camName);
            }
            
            skyboxRenderers = (from Renderer r in (FindObjectsOfType(typeof(Renderer)) as IEnumerable<Renderer>) where (r.name == "XP" || r.name == "XN" || r.name == "YP" || r.name == "YN" || r.name == "ZP" || r.name == "ZN") select r).ToArray<Renderer>();
            if (skyboxRenderers == null)
            {
                Debug.Log("CactEye 2: Logical Error: skyboxRenderers is null!");
            }

            scaledSpaceFaders = FindObjectsOfType(typeof(ScaledSpaceFader)) as ScaledSpaceFader[];
            if (scaledSpaceFaders == null)
            {
                Debug.Log("CactEye 2: Logical Error: scaledSpaceFaders is null!");
            }

            
        }


        #region Helper Functions

        /*
         * Function name: UpdateTexture
         * Input: None
         * Output: A fully rendered texture of what's through the telescope.
         * Purpose: This function will produce a single frame texture of what image is being looked
         * at through the telescope. 
         * Note: Need to modify behavior depending on what processor is currently active.
         */
        public Texture2D UpdateTexture(CactEyeProcessor CPU, RenderTexture RT, Texture2D Output)
        {

            RenderTexture CurrentRT = RenderTexture.active;
            RenderTexture.active = RT;
            //Update position of the cameras
            foreach (Camera Cam in CameraObject)
            {
                if (Cam != null)
                {
                    Cam.transform.position = CameraTransform.position;
                    Cam.transform.up = CameraTransform.up;
                    Cam.transform.forward = CameraTransform.forward;
                    if (!RotationLock)
                    {
                        Cam.transform.rotation = CameraTransform.rotation;
                    }
                    Cam.fieldOfView = FieldOfView;
                    Cam.targetTexture = RT;
                }
                else
                {
                    Debug.Log("CactEye 2: " + Cam.name.ToString() + " was not found!");
                }
            }
            foreach (Camera Cam in CameraObject)
            {
                if (Cam.name.Contains("ScaledSpace"))
                {
                    foreach (Renderer r in skyboxRenderers)
                    {
                        r.enabled = false;
                    }
                    foreach (ScaledSpaceFader s in scaledSpaceFaders)
                    {
                        s.r.enabled = true;
                    }
                    Cam.clearFlags = CameraClearFlags.Depth;
                    Cam.farClipPlane = 3e15f;
                    Cam.Render();
                    foreach (Renderer r in skyboxRenderers)
                    {
                        r.enabled = true;
                    }
                 }
                else
                {
                    Cam.Render();
                }
            }


            Output.ReadPixels(new Rect(0, 0, Output.width, Output.height), 0, 0);
            Output = CPU.ApplyFilter("Standard", Output);
            Output.Apply();
            RenderTexture.active = CurrentRT;
            return Output;
        }

        public Texture2D UpdateTexture(CactEyeProcessor CPU)
        {
            if (CPU)
            {
                return UpdateTexture(CPU, ScopeRenderTexture, ScopeTexture2D);
            }
            else
            {
                return new Texture2D(CameraWidth, CameraHeight);
            }
        }

        public Texture2D TakeScreenshot(CactEyeProcessor CPU)
        {
            //Just for right now
            return UpdateTexture(CPU, FullResolutionTexture, FullTexture2D);
        }

        /*
         * Function name: GetCameraByName
         * Purpose: This returns the camera specified by the input "name." Copied and pasted
         * from Rastor Prop Monitor.
         */
        private Camera GetCameraByName(string name)
        {
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.name == name)
                {
                    return cam;
                }
            }
            return null;
        }
        
        /*
         * Function name: CameraSetup
         * Purpose: This will make a copy of the specified camera. Taken from
         * Rastor Prop Monitor.
         */
        private void CameraSetup(string SourceName)
        {

            if (CameraObject == null)
            {
                Debug.Log("CactEye 2: Logical Error 2: The Camera Object is null. The mod author needs to perform a code review.");
            }
            else if (GetCameraByName(SourceName) == null)
            {
                if(CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Camera Not Found: " + SourceName);
                }
                return;
            }
            else
            {
                Camera newCam;
                GameObject CameraBody = new GameObject("CactEye " + SourceName);
                if (CameraBody == null)
                {
                    Debug.Log("CactEye 2: logical Error: CameraBody was null!");
                }
                CameraBody.name = "CactEye 2 " + SourceName;
                newCam = CameraBody.AddComponent<Camera>();
                if (newCam == null)
                {
                    Debug.Log("CactEye 2: Logical Error 1: CameraBody.AddComponent returned null! If you do not have Visual Enhancements installed, then this error can be safely ignored.");
                }
                newCam.CopyFrom(GetCameraByName(SourceName));
                newCam.enabled = true;
                newCam.targetTexture = ScopeRenderTexture;
                
                if (SourceName != "GalaxyCamera")
                {
                    newCam.transform.position = CameraTransform.position;
                    newCam.transform.forward = CameraTransform.forward;
                    newCam.transform.rotation = CameraTransform.rotation;
                    newCam.fieldOfView = FieldOfView;
                    newCam.farClipPlane = 3e15f;
                }
                if(CactEyeConfig.DebugMode)
                {
                    Debug.Log("CactEye 2: Adding Camera " + newCam.name);
                }
                CameraObject.Add(newCam);
                //Debug.Log("CactEye 2: Debug: Camera[" + Index.ToString() + "]: " + CameraObject[Index].cullingMask.ToString());
            }
        }

        /*
         * Function name: UpdatePosition
         * Purpose: This will update the local position data from the parent part.
         */
        public void UpdatePosition(Transform Position)
        {
            this.CameraTransform = Position;
        }

        public Camera GetOverlayCamera()
        {
            foreach(Camera Cam in CameraObject)
            {
                if (Cam.name.Contains("00"))
                {
                    return Cam;
                }
            }
            return null;
        }

        #endregion

    }
}
