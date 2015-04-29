using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Contracts;
using Contracts.Parameters;
using FinePrint;
using FinePrint.Contracts;
using KSP;
using KSPAchievements;

namespace CatEyeContracts
{
    public class NewTelescopeContract: Contract
    {

        private bool bCanBeCancelled = true;
        private bool bCanBeDeclined = true;
        private bool DebugMode = true;

        private string HashString = "";
        private string Description = "";
        private string Synopsis = "";
        private string MessageComplete = "";

        //private string ConfigFile = "NewTelescope.cfg";

        private ConfigNode ConfigurationFile;

        /* ************************************************************************************************
         * Function Name: Generate
         * Input: None
         * Output: None
         * Purpose: This function will generate a new contract requiring the player to put into orbit
         * a new space telescope.
         * ************************************************************************************************/
        protected override bool Generate()
        {
            //Load configuration data
            try
            {
                LoadConfigurationData();
            }
            catch (Exception e)
            {
                Debug.LogError("CactEye 2: Exception 6: Was not able to load configuration data for the New Telescope Contract!");
                Debug.LogException(e);
            }

            //Local variable declarations
            float Difficulty = 0f, RewardMultiplier = 1f, FundsMultiplier = 1f, ScienceMultiplier = 1f, ReputationMultiplier = 1f;
            double TargetEccentricity = 0, Deviation = 10;
            FinePrint.Utilities.OrbitType TargetOrbitType = FinePrint.Utilities.OrbitType.EQUATORIAL;
            CelestialBody TargetCelestialBody = Planetarium.fetch.Home;
            bool HaveFungEyeModule = false, HaveCatEyeModule = false;
            int ProcessorTechLevel = 0;

            //Check to see where we are at technology wise
            VerifyTechnology(ref HaveFungEyeModule, ref HaveCatEyeModule, ref ProcessorTechLevel);
            if (!HaveFungEyeModule && !HaveCatEyeModule && ProcessorTechLevel == 0)
            {
                //We don't have telescopes unlocked, so abort contract generation.
                if (DebugMode) Debug.Log("CactEye 2: Contracts: Was not able to generate new telescope contract; player does not have the required techonologies.");
                return false;
            }

            //If we have telescopes unlocked, then we have a contract.
            else
            {

                //Adjust parameters based on difficulty

                /* ************************************************************************************************
                * Generate difficulty parameter for a "Trivial" contract.
                * ************************************************************************************************/
                if (this.prestige == Contract.ContractPrestige.Trivial)
                {
                    Deviation = FinePrint.ContractDefs.Satellite.TrivialDeviation;
                    Difficulty = FinePrint.ContractDefs.Satellite.TrivialDifficulty;
                }

                /* ************************************************************************************************
                * Generate difficulty parameter for a "Significant" contract.
                * ************************************************************************************************/
                else if (this.prestige == Contract.ContractPrestige.Significant)
                {
                    Difficulty = FinePrint.ContractDefs.Satellite.SignificantDifficulty;
                    Deviation = FinePrint.ContractDefs.Satellite.SignificantDeviation;
                }

                /* ************************************************************************************************
                * Generate difficulty parameter for an "Exceptional" contract.
                * ************************************************************************************************/
                else if (this.prestige == Contract.ContractPrestige.Exceptional)
                {
                    Difficulty = FinePrint.ContractDefs.Satellite.ExceptionalDifficulty;
                    Deviation = FinePrint.ContractDefs.Satellite.ExceptionalDeviation;
                }

                //Add contract parameters
                this.AddParameter(new FinePrint.Contracts.Parameters.ProbeSystemsParameter(), null);

                
                this.AddParameter(new FinePrint.Contracts.Parameters.PartRequestParameter(new ConfigNode("Have a processor on the telescop.")), null);

                int OrbitDiceRoll = (new System.Random().Next(0, 65000)) % 5;
                switch (OrbitDiceRoll)
                {
                    case 0: TargetOrbitType = FinePrint.Utilities.OrbitType.EQUATORIAL;     break;
                    case 1: TargetOrbitType = FinePrint.Utilities.OrbitType.POLAR;          break;
                    case 2: TargetOrbitType = FinePrint.Utilities.OrbitType.SYNCHRONOUS;    break;
                    case 3: TargetOrbitType = FinePrint.Utilities.OrbitType.STATIONARY;     break;
                    case 4: TargetOrbitType = FinePrint.Utilities.OrbitType.RANDOM;         break;
                }

                //Generate the target orbit
                Orbit TargetOrbit = FinePrint.Utilities.CelestialUtilities.GenerateOrbit(
                        TargetOrbitType,
                        650,
                        TargetCelestialBody,
                        Difficulty,
                        TargetEccentricity);

                base.AddKeywords(new string[] { "deploysatellite" });
                this.AddParameter(new FinePrint.Contracts.Parameters.SpecificOrbitParameter(
                    TargetOrbitType,
                    TargetOrbit.inclination,
                    TargetOrbit.eccentricity,
                    TargetOrbit.semiMajorAxis,
                    TargetOrbit.LAN,
                    TargetOrbit.argumentOfPeriapsis,
                    TargetOrbit.meanAnomalyAtEpoch,
                    TargetOrbit.epoch,
                    TargetCelestialBody,
                    Difficulty,
                    Deviation));


                this.AddParameter(new FinePrint.Contracts.Parameters.StabilityParameter(), null);

                if (TargetOrbitType == FinePrint.Utilities.OrbitType.POLAR)
                {
                    FundsMultiplier *= FinePrint.ContractDefs.Satellite.Funds.PolarMultiplier;
                    ScienceMultiplier *= FinePrint.ContractDefs.Satellite.Science.PolarMultiplier;
                    ReputationMultiplier *= FinePrint.ContractDefs.Satellite.Reputation.PolarMultiplier;
                }

                if (TargetOrbitType == FinePrint.Utilities.OrbitType.STATIONARY || TargetOrbitType == FinePrint.Utilities.OrbitType.SYNCHRONOUS)
                {
                    RewardMultiplier += 0.5f;
                }

                //Set Expiration date and deadline
                base.SetExpiry(
                    FinePrint.ContractDefs.Satellite.Expire.MinimumExpireDays,
                    FinePrint.ContractDefs.Satellite.Expire.MaximumExpireDays);
                base.SetDeadlineDays(FinePrint.ContractDefs.Satellite.Expire.DeadlineDays, TargetCelestialBody);

                //Set funds rewards
                base.SetFunds(
                    Mathf.Round(FinePrint.ContractDefs.Satellite.Funds.BaseAdvance * FundsMultiplier),
                    Mathf.Round(FinePrint.ContractDefs.Satellite.Funds.BaseReward * FundsMultiplier),
                    Mathf.Round(FinePrint.ContractDefs.Satellite.Funds.BaseFailure * FundsMultiplier),
                    TargetCelestialBody);

                //Set science rewards
                base.SetScience(Mathf.Round(FinePrint.ContractDefs.Satellite.Science.BaseReward * ScienceMultiplier), TargetCelestialBody);

                //Set reputation rewards
                base.SetReputation(
                    Mathf.Round(FinePrint.ContractDefs.Satellite.Reputation.BaseReward * ReputationMultiplier),
                    Mathf.Round(FinePrint.ContractDefs.Satellite.Reputation.BaseFailure * ReputationMultiplier),
                    TargetCelestialBody);

                return true;
            }
        }

        public override bool CanBeCancelled()
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: CanBeCancelled Call!");
            return bCanBeCancelled;
        }

        public override bool CanBeDeclined()
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: CanBeDeclined Call!");
            return bCanBeDeclined;
        }

        protected override string GetHashString()
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: GetHashString Call!");
            return HashString;
        }

        protected override string GetDescription()
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: GetDescription Call!");
            return Description;
        }

        protected override string GetSynopsys()
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: GetSynopsis Call!");
            return Synopsis;
        }

        protected override string MessageCompleted()
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: MessageCompleted Call!");
            return MessageComplete;
        }

        protected override void OnLoad(ConfigNode node)
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: OnLoad Call!");
            base.OnLoad(node);
        }

        protected override void OnSave(ConfigNode node)
        {
            if (DebugMode) Debug.Log("CactEye 2: Contracts: OnSave Call!");
            base.OnSave(node);
        }

        public override bool MeetRequirements()
        {
            //if (DebugMode) Debug.Log("CactEye 2: Contracts: MeetRequiredments Called!");
            return true;
        }

        protected void LoadConfigurationData()
        {
            var Database = GameDatabase.Instance;
            var Settings = Database.GetConfigNodes("CactEyeNewTelescopeContract").LastOrDefault();

            if (Settings != null)
            {
                //Enable/Disable DebugMode depending on Configuration file settings.
                if (!bool.TryParse(ConfigurationFile.GetValue("DebugMode"), out DebugMode))
                {
                    Debug.LogError("CactEye 2: Error #: Failed to retrieve the DebugMode attribute from New Telescope Configuration file!");
                }

                //Determine if the contract can be cancelled or not.
                if (!bool.TryParse(ConfigurationFile.GetValue("CanBeCancelled"), out bCanBeCancelled))
                {
                    Debug.LogError("CactEye 2: Error #: Failed to retrieve the CanBeCancelled attribute from New Telescope Configuration file!");
                }

                //Determine if the contract can be declined or not.
                if (!bool.TryParse(ConfigurationFile.GetValue("CanBeDeclined"), out bCanBeDeclined))
                {
                    Debug.LogError("CactEye 2: Error #: Failed to retrieve the CanBeDeclined attribute from New Telescope Configuration file!");
                }

                HashString      = ConfigurationFile.GetValue("HashString");
                Description     = ConfigurationFile.GetValue("Description");
                Synopsis        = ConfigurationFile.GetValue("Synopsis");
                MessageComplete = ConfigurationFile.GetValue("MessageComplete");
            }

            else
            {
                Debug.LogError("CactEye 2: Error #: Was not able to load New Telescope configuration file!");
            }
        }

        /* ************************************************************************************************
         * Function Name: OnUpdate
         * Input: None
         * Output: 
         *      (bool) HaveFungEyeModule: a boolean value stating whether or not the FungEye optics part
         *      is available to the player.
         *      (bool) HaveCactEyeModule: a boolean value stating whether or not the CactEye optics part
         *      is available to the player.
         *      (int) ProcessorTechLevel: an integer stating what level processor is available to the 
         *      player.
         * Purpose: This function will check to verify what technology related to the CactEye telescpe is
         * available to the player. All arguements are passed by reference and serve as the outputs for
         * this function.
         * ************************************************************************************************/
        protected void VerifyTechnology(ref bool HaveFungEyeModule, ref bool HaveCactEyeModule, ref int ProcessorTechLevel)
        {
            if (ResearchAndDevelopment.GetTechnologyState("spaceExploration") == RDTech.State.Available)
            {
                if (DebugMode) { 
                    Debug.Log("CactEye 2: New Telescope Contract: Player has the FungEye optics module unlocked."); 
                    Debug.Log("CactEye 2: Player is at Telescope tech level 1.");
                }
                HaveFungEyeModule = true;
                ProcessorTechLevel = 1;
            }

            if (ResearchAndDevelopment.GetTechnologyState("largeElectronics") == RDTech.State.Available)
            {
                if (DebugMode) Debug.Log("CactEye 2: New Telescope: Player has the CactEye optics module unlocked.");
                HaveCactEyeModule = true;
            }

            if (ResearchAndDevelopment.GetTechnologyState("electronics") == RDTech.State.Available)
            {
                if (DebugMode) Debug.Log("CactEye 2: Player is at Telescope tech level 2.");
                ProcessorTechLevel = 2;
            }

            if (ResearchAndDevelopment.GetTechnologyState("experimentalScience") == RDTech.State.Available)
            {
                if (DebugMode) Debug.Log("CactEye 2: Player is at Telescope tech level 3.");
                ProcessorTechLevel = 3;
            }
        }
    }
}
