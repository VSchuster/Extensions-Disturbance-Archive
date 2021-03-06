//  Copyright 2006-2011 University of Wisconsin, Portland State University
//  Authors:  Jane Foster, Robert M. Scheller

//using Landis.Extension.Succession.Biomass;
using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;
using System.Collections.Generic;
using System;


namespace Landis.Extension.Insects
{
    public class Defoliate
    {

        private static IEnumerable<IInsect> manyInsect;
        //---------------------------------------------------------------------

        public static void Initialize(IInputParameters parameters)
        {
            manyInsect = parameters.ManyInsect;

            // Assign the method below to the CohortDefoliation delegate in
            // biomass-cohorts/Biomass.CohortDefoliation.cs
            CohortDefoliation.Compute = DefoliateCohort;

        }


        //---------------------------------------------------------------------
        // This method replaces the delegate method.  It is called every year when
        // ACT_ANPP is calculated, for each cohort.  Therefore, this method is operating at
        // an ANNUAL time step and separate from the normal extension time step.

        // The method calculates a fraction of leaf biomass lost to defoliation.

        public static double DefoliateCohort(ICohort cohort, ActiveSite site, int siteBiomass)
        {
            //PlugIn.ModelCore.Log.WriteLine("   Calculating insect defoliation...");

            int sppIndex = cohort.Species.Index;
            double totalDefoliation = 0.0;

            foreach(IInsect insect in manyInsect)
            {
                if (!insect.ActiveOutbreak)
                {
                    //if (PlugIn.ModelCore.CurrentTime >= 8)
                    //    PlugIn.ModelCore.Log.WriteLine("   {0} is NOT active.  ", insect.Name);
                    continue;
                }

                //if (PlugIn.ModelCore.CurrentTime >= 8)
                //    PlugIn.ModelCore.Log.WriteLine("   Begin defoliation within Succession... ");


                double defoliation = 0.0;
                double weightedDefoliation = 0.0;
                int suscIndex = insect.SppTable[sppIndex].Susceptibility - 1;


                // Get the Neighborhood GrowthReduction Density
                double meanNeighborhoodDefoliation = 0.0;
                int neighborCnt = 0;

                // If it is the first year, the neighborhood growth reduction
                // will have been initialized in Outbreak.InitializeDefoliationPatches

                if(insect.NeighborhoodDefoliation[site] > 0)
                {
                    //PlugIn.ModelCore.Log.WriteLine("   First Year of Defoliation:  Using initial patch defo={0:0.00}.", SiteVars.NeighborhoodDefoliation[site]);
                    meanNeighborhoodDefoliation = insect.NeighborhoodDefoliation[site];
                }

                // If not the first year, calculate mean neighborhood defoliation based on the
                // previous year.
                else
                {
                    double sumNeighborhoodDefoliation = 0.0;

                    //PlugIn.ModelCore.Log.WriteLine("Look at the Neighbors... ");
                    foreach (RelativeLocation relativeLoc in insect.Neighbors)
                    {
                        Site neighbor = site.GetNeighbor(relativeLoc);
                        if (neighbor != null && neighbor.IsActive)
                        {
                            neighborCnt++;
                            sumNeighborhoodDefoliation += insect.LastYearDefoliation[neighbor];
                        }
                    }

                    if(neighborCnt > 0.0)
                        meanNeighborhoodDefoliation = sumNeighborhoodDefoliation / (double) neighborCnt;

                }  //endif

                if(meanNeighborhoodDefoliation > 1.0 || meanNeighborhoodDefoliation < 0)
                {
                    PlugIn.ModelCore.UI.WriteLine("MeanNeighborhoodDefoliation={0}; NeighborCnt={1}.", meanNeighborhoodDefoliation, neighborCnt);
                    throw new ApplicationException("Error: Mean Neighborhood GrowthReduction is not between 1.0 and 0.0");
                }

                // First assume that there are no neighbors whatsoever:
                DistributionType dist = insect.SusceptibleTable[suscIndex].Distribution_0.Name;
                double value1 = insect.SusceptibleTable[suscIndex].Distribution_0.Value1;
                double value2 = insect.SusceptibleTable[suscIndex].Distribution_0.Value2;

                if(meanNeighborhoodDefoliation <= 1.0 && meanNeighborhoodDefoliation >= 0.8)
                {
                    dist = insect.SusceptibleTable[suscIndex].Distribution_80.Name;
                    value1 = insect.SusceptibleTable[suscIndex].Distribution_80.Value1;
                    value2 = insect.SusceptibleTable[suscIndex].Distribution_80.Value2;
                }
                else if(meanNeighborhoodDefoliation < 0.8 && meanNeighborhoodDefoliation >= 0.6)
                {
                    dist = insect.SusceptibleTable[suscIndex].Distribution_60.Name;
                    value1 = insect.SusceptibleTable[suscIndex].Distribution_60.Value1;
                    value2 = insect.SusceptibleTable[suscIndex].Distribution_60.Value2;
                }
                else if(meanNeighborhoodDefoliation < 0.6 && meanNeighborhoodDefoliation >= 0.4)
                {
                    dist = insect.SusceptibleTable[suscIndex].Distribution_40.Name;
                    value1 = insect.SusceptibleTable[suscIndex].Distribution_40.Value1;
                    value2 = insect.SusceptibleTable[suscIndex].Distribution_40.Value2;
                }
                else if(meanNeighborhoodDefoliation < 0.4 && meanNeighborhoodDefoliation >= 0.2)
                {
                    dist = insect.SusceptibleTable[suscIndex].Distribution_20.Name;
                    value1 = insect.SusceptibleTable[suscIndex].Distribution_20.Value1;
                    value2 = insect.SusceptibleTable[suscIndex].Distribution_20.Value2;
                }
                else if(meanNeighborhoodDefoliation < 0.2 && meanNeighborhoodDefoliation >= 0.0)
                {
                    dist = insect.SusceptibleTable[suscIndex].Distribution_0.Name;
                    value1 = insect.SusceptibleTable[suscIndex].Distribution_0.Value1;
                    value2 = insect.SusceptibleTable[suscIndex].Distribution_0.Value2;
                }

                // Next, ensure that all cohorts of the same susceptibility class
                // receive the same level of defoliation.

                if(insect.HostDefoliationByYear[site].ContainsKey(PlugIn.ModelCore.CurrentTime))
                {
                    if(insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime][suscIndex] <= 0.00000000)
                    {
                        defoliation = Distribution.GenerateRandomNum(dist, value1, value2);
                        insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime][suscIndex] = defoliation;
                    }
                    else
                        defoliation = insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime][suscIndex];
                }
                else
                {
                    insect.HostDefoliationByYear[site].Add(PlugIn.ModelCore.CurrentTime, new Double[3]{0.0, 0.0, 0.0});
                    defoliation = Distribution.GenerateRandomNum(dist, value1, value2);

                    insect.HostDefoliationByYear[site][PlugIn.ModelCore.CurrentTime][suscIndex] = defoliation;
                }


                // Alternatively, allow defoliation to vary even among cohorts and species with
                // the same susceptibility.
                if(defoliation > 1.0 || defoliation < 0)
                {
                    PlugIn.ModelCore.UI.WriteLine("DEFOLIATION TOO BIG or SMALL:  {0}, {1}, {2}, {3}.", dist, value1, value2, defoliation);
                    throw new ApplicationException("Error: New defoliation is not between 1.0 and 0.0");
                }

                //PlugIn.ModelCore.Log.WriteLine("Cohort age={0}, species={1}, suscIndex={2}, defoliation={3}.", cohort.Age, cohort.Species.Name, (suscIndex -1), defoliation);

                //weightedDefoliation = (defoliation * ((double) (cohort.LeafBiomass + cohort.WoodBiomass) / (double) siteBiomass)); //AMK Old LBI eqn
                //weightedDefoliation = (Math.Min((1 - totalDefoliation), defoliation) * ((double)cohort.LeafBiomass / (double)siteBiomass)); //AMK 6/4 update from Jane's BiomassInsects
                double capDefol = Math.Min((1 - totalDefoliation), defoliation);
                double weight = (double)(int)(cohort.WoodBiomass + cohort.LeafBiomass) / (double)siteBiomass;  //RMS 8/7 Jane says should be WoodBiomass
                weightedDefoliation = capDefol * weight; 
                
                //if (PlugIn.ModelCore.CurrentTime > 8)
                //PlugIn.ModelCore.Log.WriteLine("Cohort age={0}, species={1}, suscIndex={2}, cohortDefoliation={3}, weightedDefolation={4}.", cohort.Age, cohort.Species.Name, (suscIndex+1), defoliation, weightedDefoliation);

                insect.ThisYearDefoliation[site] += weightedDefoliation;

                if(insect.ThisYearDefoliation[site] > 1.0) // FIXME - siteBiomass should be calculated before cohort growth and passed to DefoliateCohort with the same value for all cohorts, as it is the weights do not necessarily sum to 1.0.
                {
                    insect.ThisYearDefoliation[site] = 1.0;
                }
                totalDefoliation += defoliation;

                //if (weightedDefoliation > 0.0 && PlugIn.ModelCore.CurrentTime > 8)
                //    PlugIn.ModelCore.Log.WriteLine("   Weighted defoliation due to insect defoliation = {0:0.00}", weightedDefoliation);
                
            }  //end insect loop

            if(totalDefoliation > 1.0)  // Cannot exceed 100% defoliation
                totalDefoliation = 1.0;

            if(totalDefoliation > 1.0 || totalDefoliation < 0)
            {
                PlugIn.ModelCore.UI.WriteLine("Cohort Total Defoliation = {0:0.00}.  Site R/C={1}/{2}.", totalDefoliation, site.Location.Row, site.Location.Column);
                throw new ApplicationException("Error: Total Defoliation is not between 1.0 and 0.0");
            }


            return totalDefoliation;

        }


    }

}
