using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Virtual.Interface.Golf.Models
{
    public class Player
    {
        public Int32 PlayerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TvName { get; set; }
        public Uri Headshot { get; set; }
        public Country Country { get; set; }
        public int Age { get; set; }
        public int WorldRank { get; set; }
        public int PGATourWins { get; set; }
        public int EuroTourWins { get; set; }
        public int MajorWins { get; set; }
        public string BestTourneyFinish { get; set; }
        public string BestMajorFinish { get; set; }
        public string TotalScoreStr { get; set; }
        public int TotalScore { get; set; }
        public string TodayScoreStr { get; set; }
        public decimal? TodayScore { get; set; }
        public int Thru { get; set; }
        public string ThruStr { get; set; }
        public string ThruColor { get; set; }
        public int Position { get; set; }
        public string PositionStr { get; set; }
        public string DisplayPosition { get; set; }
        public bool Notable { get; set; }
        public string Clothing { get; set; }
        public string HighlightColor { get; set; }
        public int Status { get; set; }
        public List<Shot> Shots { get; set; }
    }
}
