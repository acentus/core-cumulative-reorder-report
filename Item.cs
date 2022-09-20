//
// Copyright (c) 2022 by Acentus
// Developed by: Luis Cabrera
// gigocabrera@outlook.com
//

namespace CoreCumulativeReorderReport
{
    internal class Item
    {
        public int Id { get; set; }
        public string SalesRep { get; set; }
        public int TotalActivePatients { get; set; }
        public int TotalExpVisitNotes { get; set; }
        public int TotalExpSWO { get; set; }
        public double Renewal87 { get; set; }
        public double Renewal93 { get; set; }
        public int CurrentRenewal { get; set; }
        public int CurrentRenewalPercent { get; set; }
        public int TotalOrdersThisMonth { get; set; }
    }
}