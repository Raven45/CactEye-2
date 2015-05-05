using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{


    /* ************************************************************************************************
    * Class Name: CactEyeAsteroidSpawner_Flight
    * Purpose: This will allow CactEye's custom asteroid spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class CactEyeAsteroidSpawner_Flight : CactEyeAsteroidSpawner
    {
    }

    /* ************************************************************************************************
    * Class Name: CactEyeAsteroidSpawner_SpaceCentre
    * Purpose: This will allow CactEye's custom asteroid spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class CactEyeAsteroidSpawner_SpaceCentre : CactEyeAsteroidSpawner
    {
    }

    /* ************************************************************************************************
    * Class Name: CactEyeAsteroidSpawner_TrackingStation
    * Purpose: This will allow CactEye's custom asteroid spawner to run in multiple specific scenes. 
    * Shamelessly stolen from Starstrider42, who shamelessly stole it from Trigger Au.
    * Thank you!
    * ************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    internal class CactEyeAsteroidSpawner_TrackStation : CactEyeAsteroidSpawner
    {
    }

    internal class CactEyeAsteroidSpawner: MonoBehaviour
    {


        private int GlobalDiscoveryRate;

        /* ************************************************************************************************
         * Function Name: Start
         * Input: N/A
         * Output: N/A
         * Purpose: This function will run at the start of the loaded scene. It will check to verify it is
         * loaded in the correct scene and then call a coroutine, "DelayedStart()."
         * ************************************************************************************************/
        public void Start()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.SPACECENTER ||
                HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                StartCoroutine(DelayedStart());
            }
        }

        /* ************************************************************************************************
         * Function Name: DelayedStart
         * Input: N/A
         * Output: N/A
         * Purpose: This function will run directly after the start of the loaded scene. It will calculate
         * the discovery rate, and then adjust the spawn rate of asteroids if it can.
         * ************************************************************************************************/
        public System.Collections.IEnumerator DelayedStart()
        {
            while (HighLogic.CurrentGame.scenarios[0].moduleRef == null)
            {
                yield return 0;
            }

            CalculateGlobalDiscoveryRate();
            AdjustSpawnRate();
        }

        /* ************************************************************************************************
         * Function Name: CalculateGlobalDiscoveryRate
         * Input: N/A
         * Output: N/A
         * Purpose: This function will scan for any active telescopes that are equipped with an asteroid
         * processor that is active and functioning correctly. It will then calculate what the total 
         * spawn rate should be for the asteroid spawner based on available telescopes.
         * ************************************************************************************************/
        private void CalculateGlobalDiscoveryRate()
        {

            GlobalDiscoveryRate = 100;

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {


                foreach (ProtoPartSnapshot part in vessel.protoVessel.protoPartSnapshots)
                {
                    ProtoPartModuleSnapshot cpu = part.modules.Find(n => n.moduleName == "CactEyeAsteroidProcessor");

                    if (cpu != null && bool.Parse(cpu.moduleValues.GetValue("Active")))
                    {
                        int PartDiscoveryRate = 0;
                        try
                        {
                            PartDiscoveryRate = int.Parse(cpu.moduleValues.GetValue("DiscoveryRate"));
                        }
                        catch (Exception e)
                        {
                            Debug.Log("CactEye 2: Asteroid Spawner: Was not able to retrieve a discovery rate from an active telescope");
                        }
                        //int.TryParse(cpu.moduleValues.GetValue("DiscoveryRate"), out PartDiscoveryRate);

                        GlobalDiscoveryRate -= PartDiscoveryRate;

                        if (CactEyeConfig.DebugMode)
                        {
                            Debug.Log("CactEye 2: Asteroid Spawner: Asteroid Processor Found!");
                            Debug.Log("CactEye 2: Asteroid Spawner: Discovery rate: " + PartDiscoveryRate.ToString());
                        }
                    }
                }

                //foreach (Part part in vessel.Parts)
                //{
                //    CactEyeAsteroidProcessor cpu = part.GetComponent<CactEyeAsteroidProcessor>();
                //    if (cpu != null && cpu.Active)
                //    {
                //        GlobalDiscoveryRate -= cpu.DiscoveryRate;
                //        Debug.Log("CactEye 2: Asteroid Spawner: Asteroid Processor Found!");
                //    }
                //}
            }
        }

        /* ************************************************************************************************
         * Function Name: CheckForIncompatibleMods
         * Input: N/A
         * Output: N/A
         * Purpose: This function will check to see if certain mods that are imcompatible with the 
         * Asteroid spawner are installed. If they are, CactEye will not override the spawning of asteroids.
         * This is primarily to prevent CactEye from interfering and conflicting with CustomAsteroids by
         * Starstrider42.
         * ************************************************************************************************/
        private bool CheckForIncompatibleMods()
        {
            foreach (AssemblyLoader.LoadedAssembly ASM in AssemblyLoader.loadedAssemblies)
            {
                if (ASM.dllName == "CustomAsteroids")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /* ************************************************************************************************
         * Function Name: AdjustSpawnRate
         * Input: N/A
         * Output: N/A
         * Purpose: This function will adjust the spawn rate of the stock asteroid spawner to CactEye's 
         * own calculated value, of which is based on the avialability of asteroid telescopes.
         * This function will not run if CustomAsteroids is installed at this time. Perhaps in the future,
         * this can be modified to allow CactEye and CustomAsteroids to work together, but this will require
         * some changes in CustomAsteroids first.
         * ************************************************************************************************/
        private void AdjustSpawnRate()
        {
            if (!CheckForIncompatibleMods())
            {

                try
                {
                    ScenarioDiscoverableObjects AsteroidSpawner = (ScenarioDiscoverableObjects)HighLogic.CurrentGame.scenarios.Find(scenario => scenario.moduleRef is ScenarioDiscoverableObjects).moduleRef;

                    AsteroidSpawner.spawnOddsAgainst = GlobalDiscoveryRate;

                    if (CactEyeConfig.DebugMode)
                    {
                        Debug.Log("CactEye 2: Asteroid Spawner: spawnOddsAgainst = " + AsteroidSpawner.spawnOddsAgainst.ToString());
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("CactEye 2: Asteroid Spawner: Was not able to adjust spawn rate; AsteroidSpawner object is null!");
                    Debug.Log(e.ToString());
                }
            }

            else
            {
                Debug.Log("CactEye 2: An incompatible mod (most likely Custom Asteroids) was detected. CactEye will not adjust the asteroid spawn rate.");
            }
        }


        /* ************************************************************************************************
         * Function Name: UpdateSpawnRate
         * Input: N/A
         * Output: N/A
         * Purpose: This function will recalculate and readjust the spawn rate of asteroids. This is to
         * allow for dynamic changes in the spawn rate as telescopes are enabled/disabled.
         * ************************************************************************************************/
        public void UpdateSpawnRate()
        {
            CalculateGlobalDiscoveryRate();
            AdjustSpawnRate();
        }
    }
}
