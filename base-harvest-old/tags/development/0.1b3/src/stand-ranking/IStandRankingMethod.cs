using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.BaseHarvest
{
    /// <summary>
    /// A method for ranking stands.
    /// </summary>
    public interface IStandRankingMethod
    {
        /// <summary>
        /// Adds a requirement which must be satified by a stand in order for
        /// the ranking method to compute its ranking.
        /// </summary>
        /// <remarks>
        /// If a stand does not meet the requirements, its rank is 0.
		/// </remarks>

		List<IRequirement> Requirements {
			get;
		}
		
        void AddRequirement(IRequirement requirement);

        //---------------------------------------------------------------------

        /// <summary>
        /// Rank the stands in a management area.
        /// </summary>
        void RankStands(List<Stand>    stands,
                        StandRanking[] rankings);
    }
}