using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CactEye2;

namespace DistantObjectHook
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DOEWrapper: MonoBehaviour
    {

        private List<CactEyeOptics> Optics = new List<CactEyeOptics>();

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
        }

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

            DistantObject.FlareDraw.SetExternalFOVControl(ExternalControl);

            if (ExternalControl)
            {
                DistantObject.FlareDraw.SetFOV(ActiveOptics.GetFOV());
            }
        }
    }
}
