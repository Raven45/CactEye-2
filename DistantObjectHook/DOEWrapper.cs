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

        private void Awake()
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

        private void Update()
        {
            bool FlareEnable = true;

            foreach (CactEyeOptics optics in Optics)
            {
                if (optics.IsMenuEnabled())
                {
                    FlareEnable = false;
                }
            }

            /*****************************************************************
             * Call to the FlareDraw class from Distant Object Enhancement.
             * **************************************************************/
        }
    }
}
