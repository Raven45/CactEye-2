using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    class CactEyeCamera: MonoBehaviour
    {
        private int CameraWidth;
        private int CameraHeight;

        public Transform CameraTransform;
        public float FieldOfView;

        private RenderTexture ScopeRenderTexture;
        private Texture2D ScopeTexture2D;

        private Camera[] CameraObject = { null, null, null };

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

            CameraWidth = (int)(Screen.width*0.4f);
            CameraHeight = (int)(Screen.height*0.4f);

            ScopeRenderTexture = new RenderTexture(CameraWidth, CameraHeight, 24);
            ScopeRenderTexture.Create();

            ScopeTexture2D = new Texture2D(CameraWidth, CameraHeight);

            CameraSetup(0, "Camera ScaledSpace");
            CameraSetup(1, "Camera 01");
            CameraSetup(2, "Camera 00");

            skyboxRenderers = (from Renderer r in (FindObjectsOfType(typeof(Renderer)) as IEnumerable<Renderer>) where (r.name == "XP" || r.name == "XN" || r.name == "YP" || r.name == "YN" || r.name == "ZP" || r.name == "ZN") select r).ToArray<Renderer>();
            scaledSpaceFaders = FindObjectsOfType(typeof(ScaledSpaceFader)) as ScaledSpaceFader[];  
        }


        #region Helper Functions

        /*
         * Function name: UpdateTexture
         * Input: None
         * Output: A fully rendered texture of what's through the telescope.
         * Purpose: This function will produce a single frame texture of what image is being looked
         * at through the telescope. 
         */
        public Texture2D UpdateTexture()
        {

            RenderTexture CurrentRT = RenderTexture.active;
            RenderTexture.active = ScopeRenderTexture;

            //Update position of the cameras
            foreach (Camera Cam in CameraObject)
            {
                //The if statement fixes a bug with the camera position and timewarp.
                if (Cam.name.Contains("0"))
                    Cam.transform.position = CameraTransform.position;
                Cam.transform.forward = CameraTransform.forward;
                Cam.transform.rotation = CameraTransform.rotation;
                Cam.fieldOfView = FieldOfView;
            }

            CameraObject[0].Render();
            foreach (Renderer r in skyboxRenderers)
            {
                r.enabled = false;
            }
            foreach (ScaledSpaceFader s in scaledSpaceFaders)
            {
                s.r.enabled = true;
            }
            CameraObject[0].clearFlags = CameraClearFlags.Depth;
            CameraObject[0].farClipPlane = 3e15f;
            CameraObject[0].Render();
            foreach (Renderer r in skyboxRenderers)
            {
                r.enabled = true;
            }
            CameraObject[1].Render();
            CameraObject[2].Render();

            ScopeTexture2D.ReadPixels(new Rect(0, 0, CameraWidth, CameraHeight), 0, 0);
            ScopeTexture2D.Apply();
            RenderTexture.active = CurrentRT;

            return ScopeTexture2D;
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
        private void CameraSetup(int Index, string SourceName)
        {

            if (CameraObject == null)
            {
                Debug.Log("CactEye 2: Logical Error 2: The Camera Object is null. The mod author needs to perform a code review.");
            }
            else
            {
                GameObject CameraBody = new GameObject("CactEye " + SourceName);
                CameraBody.name = "CactEye 2" + SourceName;
                CameraObject[Index] = CameraBody.AddComponent<Camera>();
                if (CameraObject[Index] == null)
                {
                    Debug.Log("CactEye 2: Logical Error 1: CameraBody.AddComponent returned null! If you do not have Visual Enhancements installed, then this error can be safely ignored.");
                }
                CameraObject[Index].CopyFrom(GetCameraByName(SourceName));
                //CameraObject[Index].CopyFrom(Camera.main);
                CameraObject[Index].enabled = false;
                CameraObject[Index].targetTexture = ScopeRenderTexture;

                CameraObject[Index].transform.position = CameraTransform.position;
                CameraObject[Index].transform.forward = CameraTransform.forward;
                CameraObject[Index].transform.rotation = CameraTransform.rotation;
                CameraObject[Index].fieldOfView = FieldOfView;
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

        public Camera GetCamera(int Index)
        {
            return CameraObject[Index];
        }

        #endregion

    }
}
