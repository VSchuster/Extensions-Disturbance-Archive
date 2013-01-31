//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.BaseHarvest
{
    public class UIntPixel : Pixel
    {
        public Band<uint> MapCode  = "The numeric code for each raster cell";

        public UIntPixel()
        {
            SetBands(MapCode);
        }
    }
}
