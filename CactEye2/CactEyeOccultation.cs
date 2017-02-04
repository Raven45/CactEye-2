using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


//Currently incomplete
namespace CactEye2
{
    class CactEyeOccultation: CactEyeProcessor
    {

        [KSPField(isPersistant = true)]
        public string OcculationType = "None";

        private string OccultationExperiment_Asteroid(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            return "";
        }

        private string OccultationExperiment_Planetary(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            return "";
        }

        public override string DoScience(Vector3 TargetPosition, float scienceMultiplier, float FOV, Texture2D Screenshot)
        {
            if (OcculationType == "None")
            {
                return Type + ": Occulation experiment not available";
            }

            else if (OcculationType == "Asteroid")
            {
                return OccultationExperiment_Asteroid(TargetPosition, scienceMultiplier, FOV, Screenshot);
            }

            else
            {
                return OccultationExperiment_Planetary(TargetPosition, scienceMultiplier, FOV, Screenshot);
            }
        }
    }
}
