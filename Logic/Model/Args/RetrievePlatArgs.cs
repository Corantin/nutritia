﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nutritia
{
    public class RetrievePlatArgs
    {
        public int? IdPlat { get; set; }
        public string Categorie { get; set; }
        public int? NbResultats { get; set; }
        public string Depart { get; set; }
        public bool? PlusPopulaires { get; set; }
    }
}
