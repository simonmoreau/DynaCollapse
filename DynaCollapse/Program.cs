using System;
using System.IO;
// using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;
using System.Linq;
using ExtensionMethods;
using System.Text;

namespace DynaCollapse
{
    class Program
    {

        static void Main(string[] args)
        {
            string tilesCSVPath = @"C:\Users\Simon\Downloads\WFC\tiles.csv";

            List<Tile> tiles = TilesLoader(tilesCSVPath);
            Dictionary<string, Tile> tilesDictionary = tiles.ToDictionary(x => x.Id, x => x);

            // Create a grid and fill it with empty string list
            int gridSize = 15;
            List<Tile>[,] grid = new List<Tile>[gridSize, gridSize];
            string[,] gridIds = new string[gridSize, gridSize];

            FillArray(grid, tilesDictionary.Values.ToList(), tilesDictionary["17"]);

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (i == 0 || j == 0 || i == grid.GetLength(0) - 1 || j == grid.GetLength(1) - 1)
                    {
                        // grid[i, j] = new List<Tile>();
                        // grid[i, j].Add(tilesDictionary["17"]);
                        gridIds[i, j] = grid[i, j].FirstOrDefault().Id;
                    }
                    // General case
                    else
                    {

                        List<Tile> topTiles = (j > 0) ? grid[i, j - 1] : null;
                        List<Tile> rightTiles = (i < gridSize - 1) ? grid[i + 1, j] : null;
                        List<Tile> bottomTiles = (j < gridSize - 1) ? grid[i, j + 1] : null;
                        List<Tile> leftTiles = (i > 0) ? grid[i - 1, j] : null;

                        // new List<string>()

                        List<Tile> tileIdsFromTop = AggregateAdjency(topTiles, tilesDictionary, Adjacency.BottomAdjacency);
                        List<Tile> tileIdsFromRight = AggregateAdjency(rightTiles, tilesDictionary, Adjacency.LeftAdjacency);
                        List<Tile> tileIdsFromBottom = AggregateAdjency(bottomTiles, tilesDictionary, Adjacency.TopAdjacency);
                        List<Tile> tileIdsFromLeft = AggregateAdjency(leftTiles, tilesDictionary, Adjacency.RightAdjacency);

                        List<List<Tile>> listOfLists = new List<List<Tile>>();
                        if (tileIdsFromTop != null) listOfLists.Add(tileIdsFromTop);
                        if (tileIdsFromRight != null) listOfLists.Add(tileIdsFromRight);
                        if (tileIdsFromBottom != null) listOfLists.Add(tileIdsFromBottom);
                        if (tileIdsFromLeft != null) listOfLists.Add(tileIdsFromLeft);

                        List<Tile> intersectionTiles = listOfLists
                        .Skip(1)
                        .Aggregate(new HashSet<Tile>(listOfLists.First()), (h, e) => { h.IntersectWith(e); return h; })
                        .ToList();

                        if (intersectionTiles.Count != 0)
                        {
                            // int minWeight = intersectionTiles.Min(t => t.Weight);
                            // intersectionTiles = intersectionTiles.Where(t => t.Weight == minWeight).ToList();
                            intersectionTiles.Shuffle();

                            grid[i, j] = new Tile[] { intersectionTiles.FirstOrDefault() }.ToList();
                            gridIds[i, j] = intersectionTiles.FirstOrDefault().Id;
                        }
                        else
                        {
                            grid[i, j] = new Tile[] { tilesDictionary["17"] }.ToList();
                            gridIds[i, j] = "17";
                        }

                    }
                }
            }

            string gridPath = @"C:\Users\Simon\Downloads\WFC\grid.csv";
            WriteCSV(gridPath, gridIds);

            Console.WriteLine("Hello World!");
        }


        static List<Tile> AggregateAdjency(List<Tile> adjancentTiles, Dictionary<string, Tile> tilesDictionary, Adjacency adjacency)
        {
            if (adjancentTiles != null)
            {
                List<List<Tile>> tiles = new List<List<Tile>>();

                switch (adjacency)
                {
                    case Adjacency.BottomAdjacency:
                        tiles = adjancentTiles.Select(tile => tilesDictionary.Where(k => tile.BottomAdjacency.Contains(k.Key)).ToDictionary(p => p.Key, p => p.Value).Values.ToList()).ToList();
                        break;
                    case Adjacency.LeftAdjacency:
                        tiles = adjancentTiles.Select(tile => tilesDictionary.Where(k => tile.LeftAdjacency.Contains(k.Key)).ToDictionary(p => p.Key, p => p.Value).Values.ToList()).ToList();
                        break;
                    case Adjacency.RightAdjacency:
                        tiles = adjancentTiles.Select(tile => tilesDictionary.Where(k => tile.RightAdjacency.Contains(k.Key)).ToDictionary(p => p.Key, p => p.Value).Values.ToList()).ToList();
                        break;
                    case Adjacency.TopAdjacency:
                        tiles = adjancentTiles.Select(tile => tilesDictionary.Where(k => tile.TopAdjacency.Contains(k.Key)).ToDictionary(p => p.Key, p => p.Value).Values.ToList()).ToList();
                        break;
                    default:
                        break;
                }
                return tiles.SelectMany(x => x).Distinct().ToList();
            }
            else
            {
                return null;
            }

        }

        public static void FillArray(List<Tile>[,] array, List<Tile> fillingList, Tile borderTile)
        {
            Random rnd = new Random();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (i == 0 || j == 0 || i == array.GetLength(0) - 1 || j == array.GetLength(1) - 1)
                    {
                        array[i, j] = new Tile[] { borderTile }.ToList(); ;
                    }
                    else
                    {
                        array[i, j] = fillingList;
                    }
                }
            }
        }

        static void WriteCSV(string filePath, string[,] gridIds)
        {
            //before your loop
            var csv = new StringBuilder();

            string separator = ",";

            for (int i = 0; i < gridIds.GetLength(0); i++)
            {
                string line = gridIds[0, i];
                for (int j = 1; j < gridIds.GetLength(1); j++)
                {
                    line = line + separator + gridIds[j, i];
                }
                csv.AppendLine(line);
            }
            //after your loop
            File.WriteAllText(filePath, csv.ToString());
        }

        static List<Tile> TilesLoader(string path)
        {

            List<Tile> tiles = new List<Tile>();

            //using (TextFieldParser parser = new TextFieldParser(path))
            //{
            //    parser.TextFieldType = FieldType.Delimited;
            //    parser.SetDelimiters(",");
            //    while (!parser.EndOfData)
            //    {
            //        //Process row
            //        string[] fields = parser.ReadFields();

            //        Tile tile = new Tile
            //        {
            //            Id = fields[0],
            //            Name = fields[1],
            //            DynaName = fields[2],
            //            IdAsText = fields[3],
            //            TopAdjacency = fields[4].Split(';').ToList(),
            //            RightAdjacency = fields[5].Split(';').ToList(),
            //            BottomAdjacency = fields[6].Split(';').ToList(),
            //            LeftAdjacency = fields[7].Split(';').ToList(),
            //            Weight = Convert.ToInt32(fields[8]),
            //        };

            //        tiles.Add(tile);
            //    }
            //}

            return tiles;
        }
    }

    class Tile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DynaName { get; set; }
        public string IdAsText { get; set; }
        public List<string> TopAdjacency { get; set; }
        public List<string> RightAdjacency { get; set; }
        public List<string> BottomAdjacency { get; set; }
        public List<string> LeftAdjacency { get; set; }
        public int Weight { get; set; }

    }

    public enum Adjacency
    {
        TopAdjacency,
        RightAdjacency,
        BottomAdjacency,
        LeftAdjacency
    }
}

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

