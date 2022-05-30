using Heightmap.Classes;
using System;
using System.Collections.Generic;
using System.Windows;


namespace Heightmap.Helpers
{
    internal class Parser
    {
        public static void ParseFile(HeightMapData mapData, string fileName)
        {
            if (mapData == null)
            {
                return;
            }
                       
            var coordData = new List<Coordinate>();

            // čtení řádků souboru
            var lines = System.IO.File.ReadAllLines(fileName);
            var ncols = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var nrows = lines[1].Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
            var xllcorner = lines[2].Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
            var yllcorner = lines[3].Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries);
            var cellsize = lines[4].Split(new[] { ' ' },StringSplitOptions.RemoveEmptyEntries);

            // kontrola hlavičky souboru
            Utils.CompareStr(ncols[0], "ncols");
            Utils.CompareStr(nrows[0], "nrows");
            Utils.CompareStr(xllcorner[0], "xllcorner");
            Utils.CompareStr(yllcorner[0], "yllcorner");
            Utils.CompareStr(cellsize[0], "cellsize");

            // vyplňování hlavičky HeightMapDataco ji
            mapData.Ncols= Utils.StrToInt(ncols[1]);
            mapData.Nrows= Utils.StrToInt(nrows[1]);
            mapData.Xllcorner = Utils.StrToFloat(xllcorner[1]);
            mapData.Yllcorner = Utils.StrToFloat(yllcorner[1]);
            mapData.Cellsize= Utils.StrToFloat(cellsize[1]);

            // kontrola počtu řádků
            if (lines.Length != mapData.Nrows + 5) 
            {
                throw new Exception("Nrows does not match Data format");
            }                 

            // čtení jednotlivých řádků
            string[] dataRow;

            for (int i = 0; i < mapData.Nrows; i++) 
            {                
                dataRow = lines[5 + i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // kontrola počtu sloupců
                if (dataRow.Length != mapData.Ncols)
                {
                    throw new Exception("Ncols does not match Data format");
                }                    

                ParseDataRow(coordData,dataRow, mapData.Ncols,mapData.Nrows-i-1);              
            }
            
            mapData.Data = coordData;
        }

        private static void ParseDataRow(List<Coordinate> data, string[] line, int ncols, int row) 
        {
            for (int j = 0; j < ncols; j++)
            {
                data.Add(new Coordinate
                {
                    Location = new Point(j, row),
                    Value = Utils.StrToInt(line[j])
                });
                
            }
        }       
    }
}
