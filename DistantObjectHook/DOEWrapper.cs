using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CactEye2;
using System.Reflection;

namespace DistantObjectHook
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DOEWrapper: MonoBehaviour
    {

        private List<CactEyeOptics> Optics = new List<CactEyeOptics>();
//        AssemblyLoader.LoadedAssembly distantObject;
//        MethodInfo SetExternalFOVControl;
//        MethodInfo SetFOV;

        private void Start()
        {

            if (Optics == null)
            {
                Debug.Log("CactEye 2: DOEWrapper: Uh-oh, we have a problem. If you see this error, then you're gonna have a bad day.");
            }

            else
            {
                foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                {
                    CactEyeOptics optics = p.GetComponent<CactEyeOptics>();
                    if (optics != null)
                    {
                        if (!Optics.Contains(optics))
                        {
                            Optics.Add(optics);

                        }
                    }
                }
            }

//            findDistantObjectMethods();
        }

/*
        protected void findDistantObjectMethods()
        {
            //Find the methods we need.
            foreach (AssemblyLoader.LoadedAssembly assembly in AssemblyLoader.loadedAssemblies)
            {
                if (assembly.name == "DistantObject")
                {
                    distantObject = assembly;

                    Type[] classes = assembly.assembly.GetTypes();
                    foreach (Type flareDraw in classes)
                    {
                        if (flareDraw.Name == "FlareDraw")
                        {
                            SetExternalFOVControl = flareDraw.GetMethod("SetExternalFOVControl");
                            SetFOV = flareDraw.GetMethod("SetFOV");
                        }
                    }
                }
            }

        }
*/
        private void Update()
        {
            bool ExternalControl = false;
            CactEyeOptics ActiveOptics = null;

            foreach (CactEyeOptics optics in Optics)
            {
                //Check for when optics is null, this avoids an unknown exception
                if (optics != null && optics.IsMenuEnabled())
                {
                    ExternalControl = true;
                    ActiveOptics = optics;
                }
            }

            /*
            if (SetExternalFOVControl != null && SetFOV != null)
            {
                SetExternalFOVControl.Invoke(distantObject, new object[] { ExternalControl });

                if (ExternalControl)
                    SetFOV.Invoke(distantObject, new object[] { ActiveOptics.GetFOV() });
            }
             */ 

            DistantObject.FlareDraw.SetExternalFOVControl(ExternalControl);

            if (ExternalControl)
            {
                DistantObject.FlareDraw.SetFOV(ActiveOptics.GetFOV());
            }
        }
    }
}
