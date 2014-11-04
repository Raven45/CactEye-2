using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CactEye2
{
    class CactEyeGyro: ModuleReactionWheel
    {

        [KSPField(isPersistant = false)]
        public float gyroScale = 0.1f;

        [KSPField(isPersistant = true, guiActive = true, guiUnits = "Percent", guiActiveEditor = true, guiFormat = "P1")]
        [UI_ProgressBar(minValue = 0f, maxValue = 1f, controlEnabled = true)]
        public float GyroSensitivity = 1f;

        [KSPField(isPersistant = false)]
        public float guiRate = 0.3f;

        [KSPField(isPersistant = false)]
        public int lifeSpan = 90; // in Earth days

        [KSPField(isPersistant = true)]
        float OriginalPitchTorgue;

        [KSPField(isPersistant = true)]
        float OriginalYawTorgue;

        [KSPField(isPersistant = true)]
        float OriginalRollTorgue;

        private bool IsFunctional = true;

        private double CreationTime = -1f;

        [KSPField(isPersistant = true, guiActive = true, guiUnits = "Lifetime", guiActiveEditor = false, guiFormat = "P1")]
        [UI_ProgressBar(minValue = 0f, maxValue = 1f, controlEnabled = false)]
        public float Lifetime = 1f;

        private int SecondsToEarthDays = 86400; //seconds per day

        [KSPEvent(active = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repair Gyroscope", unfocusedRange = 2)]
        public void RepairGyro()
        {
            IsFunctional = true;
            Lifetime = 1f;
            CreationTime = Planetarium.GetUniversalTime();
            wheelState = WheelState.Active;

            Events["RepairGyro"].active = false;

            base.PitchTorque = OriginalPitchTorgue;
            base.YawTorque = OriginalYawTorgue;
            base.RollTorque = OriginalRollTorgue;
            GyroSensitivity = 1f;

            Activate(null);
        }

        public void Die()
        {

            Events["RepairGyro"].active = true;

            base.PitchTorque = 0f;
            base.YawTorque = 0f;
            base.RollTorque = 0f;

            Deactivate(null);
            wheelState = WheelState.Broken;
            Lifetime = 0;
            IsFunctional = false; //RIP <3
        }

        #region "Overrides"

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            OriginalPitchTorgue = base.PitchTorque;
            OriginalYawTorgue = base.YawTorque;
            OriginalRollTorgue = base.RollTorque;

            CreationTime = Planetarium.GetUniversalTime() + ((SecondsToEarthDays * lifeSpan) * Lifetime) - (SecondsToEarthDays * lifeSpan);
            print("creationTime: " + CreationTime + " /// time: " + Planetarium.GetUniversalTime());
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (((SecondsToEarthDays * lifeSpan) + CreationTime) < Planetarium.GetUniversalTime() && IsFunctional)
            {
                Die();
            }

            if (IsFunctional)
            {
                Lifetime = (float)((CreationTime + (SecondsToEarthDays * lifeSpan) - Planetarium.GetUniversalTime()) / (SecondsToEarthDays * lifeSpan));
                base.PitchTorque = OriginalPitchTorgue * (gyroScale + ((1 - gyroScale) * GyroSensitivity));
                base.YawTorque = OriginalYawTorgue * (gyroScale + ((1 - gyroScale) * GyroSensitivity));
                base.RollTorque = OriginalRollTorgue * (gyroScale + ((1 - gyroScale) * GyroSensitivity));
            }
        }

        public override string GetInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Lifespan: " + ((GameSettings.KERBIN_TIME) ? ((lifeSpan * 4) + " Kerbin Days") : (lifeSpan + " Earth Days")));

            sb.AppendLine();

            if (PitchTorque == YawTorque && PitchTorque == RollTorque)
            {
                sb.AppendLine("Normal Torque: " + string.Format("{0:0.0##}", PitchTorque));
                sb.AppendLine("Fully Reduced Torque: " + string.Format("{0:0.0##}", PitchTorque * gyroScale));
            }
            else
            {
                sb.AppendLine("Pitch Torque: " + string.Format("{0:0.0##}", PitchTorque));
                sb.AppendLine("Yaw Torque: " + string.Format("{0:0.0##}", PitchTorque));
                sb.AppendLine("Roll Torque: " + string.Format("{0:0.0##}", RollTorque));
                sb.AppendLine("Fully Reduced Scale: " + string.Format("{0:0.0##}", gyroScale));
            }

            if (guiRate != -1)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#99ff00ff>Requires:</color>");
                sb.AppendLine("- ElectricCharge: " + ((guiRate < 1) ? (string.Format("{0:0.0##}", guiRate * 60) + "/min.") : (string.Format("{0:0.0##}", guiRate) + "/sec.")));
            }

            return sb.ToString();
        }

        #endregion
    }
}
