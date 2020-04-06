﻿namespace W3ChampionsStatisticService.MatchEvents
{
    public class LeagueConstellationChangedEvent
    {
        public int id { get; set; }
        public string name { get; set; }
        public int order { get; set; }
    }

    public class RankingChangedEvent
    {
        public int id { get; set; }
        public Rank ranks { get; set; }
    }

    public class Rank
    {
        public string id { get; set; }
        public int rp { get; set; }
    }
}