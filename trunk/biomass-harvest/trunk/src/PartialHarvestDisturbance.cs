// Copyright 2008-2010 Green Code LLC, Portland State University
// Authors:  James B. Domingo, Robert M. Scheller,  
 

using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;
using Landis.Extension.BaseHarvest;

using System.Collections.Generic;

using BaseHarvest = Landis.Extension.BaseHarvest;


namespace Landis.Extension.BiomassHarvest
{
    /// <summary>
    /// A biomass disturbance that handles partial thinning of cohorts.
    /// </summary>
    public class PartialHarvestDisturbance
        : IDisturbance
    {
        private static PartialHarvestDisturbance singleton;
        private static IDictionary<ushort, int>[] reductions;

        private static ActiveSite currentSite;
        private static Stand originalStand;  // the originating stand

        private static int numberCohortsReduced;
        private static double capacityReduction;

        //---------------------------------------------------------------------

        ActiveSite Landis.Library.BiomassCohorts.IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.Type;
            }
        }

        //---------------------------------------------------------------------

        static PartialHarvestDisturbance()
        {
            singleton = new PartialHarvestDisturbance();
        }

        //---------------------------------------------------------------------

        public PartialHarvestDisturbance()
        {
        }

        //---------------------------------------------------------------------

        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        {
            int reduction;
            if (reductions[cohort.Species.Index].TryGetValue(cohort.Age, out reduction))
            {

                SiteVars.BiomassRemoved[currentSite] += reduction;
                SiteVars.CohortsPartiallyDamaged[currentSite]++;

                if (originalStand.LastPrescription.PreventEstablishment)
                {
                    numberCohortsReduced++;
                    capacityReduction += (double) reduction / (double) cohort.Biomass;
                }

                // Record any cohort touched, not just killed:
                BaseHarvest.SiteVars.Stand[currentSite].UpdateDamageTable(cohort.Species.Name);

                return reduction;
            }
            else
                return 0;
        }

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            reductions = new IDictionary<ushort, int>[PlugIn.ModelCore.Species.Count];
            for (int i = 0; i < reductions.Length; i++)
                reductions[i] = new Dictionary<ushort, int>();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Records the biomass reduction for a particular cohort.
        /// </summary>
        public static void RecordBiomassReduction(ICohort cohort,
                                                  int     reduction)
        {
            //PlugIn.ModelCore.Log.WriteLine("Recording reduction:  {0:0.0}/{1:0.0}/{2}.", cohort.Species.Name, cohort.Age, reduction);
            reductions[cohort.Species.Index][cohort.Age] = reduction;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces the biomass of cohorts that have been marked for partial
        /// reduction.
        //  Original stand is the originating stand in cases where there is stand spreading.
        /// </summary>
        public static void ReduceCohortBiomass(ActiveSite site, Stand stand)
        {
            currentSite = site;
            originalStand = stand;
            numberCohortsReduced = 0;
            capacityReduction = 0.0;

            //PlugIn.ModelCore.Log.WriteLine("ReducingCohortBiomass NOW!");

            SiteVars.Cohorts[site].ReduceOrKillBiomassCohorts(singleton);

            //The function above will have gone through all the cohorts.  Now summarize
            //site level information.

            if(SiteVars.BiomassRemoved[site] > 0)
                BaseHarvest.SiteVars.Prescription[site] = originalStand.LastPrescription;

            if(SiteVars.BiomassRemoved[site] > 0 && BaseHarvest.SiteVars.CohortsDamaged[site] == 0)
                originalStand.LastAreaHarvested += PlugIn.ModelCore.CellArea;

            if (originalStand.LastPrescription.SpeciesToPlant != null)
                Landis.Library.Succession.Reproduction.ScheduleForPlanting(originalStand.LastPrescription.SpeciesToPlant, site);

            if (originalStand.LastPrescription.PreventEstablishment)
                SiteVars.CapacityReduction[site] = capacityReduction / (double) numberCohortsReduced;

            for (int i = 0; i < reductions.Length; i++)
                reductions[i].Clear();
        }
    }
}
