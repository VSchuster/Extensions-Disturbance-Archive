﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.LeafBiomassHarvest
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time {set; get;}

        [DataFieldAttribute(Desc = "Management Area")]
        public uint ManagementArea { set; get; }

        [DataFieldAttribute(Desc = "Prescription Name")]
        public string Prescription { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Harvested Sites")]
        public int TotalHarvestedSites { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Cohorts Complete Harvest")]
        public int TotalCohortsCompleteHarvest { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Cohorts Partial Harvest")]
        public int TotalCohortsPartialHarvest { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Species Cohorts Harvested by Species", SppList = true)]
        public double[] CohortsHarvested_ { set; get; }


    }
}
