using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IO;
using System.Globalization;

namespace EliteR2R
{
    public class ESystem
    {
        public ESystem(string systemstar)
        {
            bodies = new List<string>();
            starname = systemstar;
            x = 0;
            y = 0;
            z = 0;
        }

        public ESystem LookUpSystem()
        {
            return 
        }

        // STORAGE 
        //  for Property
        private string starname;
        //  for instance variable
        public List<string> bodies;
        public int x;
        public int y;
        public int z;
    }
}