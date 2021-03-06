namespace Landis.Extension.BaseHarvest
{
    /// <summary>
    /// A stand ranking method where the oldest stands are harvested first.
    /// </summary>
    public class MaxCohortAge
        : StandRankingMethod
    {
        public MaxCohortAge()
        {
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the rank for a stand.
        /// </summary>
        /// <remarks>
        /// The stand's rank is its age.
        /// </remarks>
        protected override double ComputeRank(Stand stand, int i)
        {
            return stand.Age;
        }
    }
}
