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
    class Program
    {
        static void Main(string[] args)
        {
            // default system and radius
            string systemName = null;
            int radius = 0;
            TextInfo textInfo = new CultureInfo("en-UK", false).TextInfo; // set region 

            // Test if input arguments were supplied:
            if (args.Length != 4)
            {
                System.Console.WriteLine(@"Number of arguments must be 2");
                return;
            }
            else
            {
                // 4 supplied
                if (args[0] == "-system")   // fragile case sensitive
                {
                    // -system was first"
                    systemName = textInfo.ToTitleCase(args[1]);
                    radius = Int32.Parse(args[3]);
                }
                else
                {
                    // radius was first
                    systemName = textInfo.ToTitleCase(args[3]);
                    radius = Int32.Parse(args[1]);
                }
            }

            string OutFilePath = @"C:\Users\deepc\Google Drive\EDDB";

            MongoClient connection = new MongoClient("mongodb://localhost:27017");
            IMongoDatabase database = connection.GetDatabase("elite");
            IMongoCollection<BsonDocument> factions = database.GetCollection<BsonDocument>("factions");
            IMongoCollection<BsonDocument> systems = database.GetCollection<BsonDocument>("systems");

            BsonDocument origin = systems.Find(Builders<BsonDocument>.Filter.Eq("name", systemName)).FirstOrDefault();
            if ((origin.GetValue("name", new BsonString(string.Empty))).IsBsonNull)
            {
                System.Console.WriteLine(@"oops -vOrigin system not found, Please check spelling and letter case");
                return;
            }

            string origin_name = origin.GetValue("name").ToString();
            string filename = OutFilePath + @"\R2R - " + origin_name + " - " + radius + " LY Radius.csv";

            //BsonDocument nearbysystems = systems.Aggregate()
            double origin_x = origin.GetValue("x").ToDouble();
            double origin_y = origin.GetValue("y").ToDouble();
            double origin_z = origin.GetValue("z").ToDouble();

            // reference to exec query
            var aggregate = systems.Aggregate()
                .Match(new BsonDocument {
                    { "x", new BsonDocument{ { "$gte", origin_x - radius }, { "$lte", origin_x + radius } } },
                    { "y", new BsonDocument{ { "$gte", origin_y - radius }, { "$lte", origin_y + radius } } },
                    { "z", new BsonDocument{ { "$gte", origin_z - radius }, { "$lte", origin_z + radius } } },
                })
                .Lookup("bodies", "id", "system_id", "bodies");

            // header for csv file
            string header = "System Name, Distancee to Origin, Body Name, Type, Distance to Main Star, Value, Teraforming(Big Money),View";

            // Query the database
            List<BsonDocument> AllSystems = aggregate.ToList();

            Console.WriteLine(filename);
            Console.WriteLine(AllSystems.Count.ToString() + " systems found in search");
            string terraforming = "";
            double value = 0;
            double total = 0;
            string view = "";
            if (AllSystems.Count > 0)
            {
                // if the file exists delete the file
                if (File.Exists(filename))
                {
                    Console.WriteLine("Warning - removed existing file");
                    File.Delete(filename);
                }
                // Write out all lines to CSV
                using (StreamWriter CSV = new StreamWriter(filename))
                {
                    // WriteConcern the header to file
                    CSV.WriteLine(header);

                    // step through select system in the cube
                    foreach (BsonDocument starsystem in AllSystems)
                    {
                        // get x,y,z co-ords for system to calculate distance
                        double system_x = starsystem.GetValue("x").ToDouble();
                        double system_y = starsystem.GetValue("y").ToDouble();
                        double system_z = starsystem.GetValue("z").ToDouble();

                        // calculate distance to system from origin
                        double distance = (Math.Sqrt(
                                    Math.Pow((system_x - origin_x), 2) +
                                    Math.Pow((system_y - origin_x), 2) +
                                    Math.Pow((system_z - origin_x), 2)));

                        // Step through each body in the selected system
                        foreach (BsonDocument body in starsystem["bodies"].AsBsonArray)
                        {
                            view = "*"; // set the view default *
                            string type_name = (body.GetValue("type_name").ToString());

                            if (type_name == "Ammonia world" || type_name == "Black hole" || type_name == "Earth-like world" ||
                                type_name == "High metal content world" || type_name == "Neutron star" || type_name == "Supermassive black hole" ||
                                type_name == "Water world")
                            {
                                // Empty string check
                                if ((body.GetValue("terraforming_state_id", new BsonString(string.Empty))).IsBsonNull)
                                {
                                    body.Set("terraforming_state_id", 0);
                                }

                                // calculate the value of the body
                                if ((body.GetValue("terraforming_state_id")) == 0)
                                {
                                    // if terraforming_id was null (now 0)
                                    terraforming = "No";
                                    switch (body.GetValue("type_name").ToString())
                                    {
                                        case "Ammonia world": value = 133418; break;
                                        case "Black hole": value = 25819; break;
                                        case "Earth-like world": value = 261619; break;
                                        case "High metal content world": value = 13866; view = ""; break;  // Dont want to view plain HMC worlds
                                        case "Neutron star": value = 22814; break;
                                        case "Supermassive black hole": value = 600000; break;
                                        case "Water world": value = 125587; break;
                                    }
                                    break;
                                }
                                else
                                {
                                    switch (body.GetValue("terraforming_state_id").ToDouble())
                                    {
                                        case 1:
                                            terraforming = "No";
                                            switch (body.GetValue("type_name").ToString())
                                            {
                                                case "Ammonia world": value = 133418; break;
                                                case "Black hole": value = 25819; break;
                                                case "Earth-like world": value = 261619; break;
                                                case "High metal content world": value = 13866; view = ""; break;  // Dont want to view plain HMC worlds
                                                case "Neutron star": value = 22814; break;
                                                case "Supermassive black hole": value = 600000; break;
                                                case "Water world": value = 125587; break;
                                            }
                                            break;
                                        case 2:
                                            terraforming = "Yes";
                                            switch (body.GetValue("type_name").ToString())
                                            {
                                                case "Ammonia world": value = 133418; break;
                                                case "Black hole": value = 25819; break;
                                                case "Earth-like world": value = 261619; break;
                                                case "High metal content world": value = 171770; break;
                                                case "Neutron star": value = 22814; break;
                                                case "Supermassive black hole": value = 600000; break;
                                                case "Water world": value = 289587; break;
                                            }
                                            break;
                                        default:
                                            terraforming = "No";
                                            switch (body.GetValue("type_name").ToString())
                                            {
                                                case "Ammonia world": value = 133418; break;
                                                case "Black hole": value = 25819; break;
                                                case "Earth-like world": value = 261619; break;
                                                case "High metal content world": value = 13866; view = ""; break;  // Dont want to view plain HMC worlds
                                                case "Neutron star": value = 22814; break;
                                                case "Supermassive black hole": value = 600000; break;
                                                case "Water world": value = 125587; break;
                                            }
                                            break;
                                    }
                                }
                                // only increment the total if it's terraforming
                                if (body.GetValue("type_name").ToString() != "High metal content world")
                                {
                                    total = total + value;
                                }
                                string line = origin.GetValue("name") + ",\"" +
                                            distance.ToString("N0") + "\"," +
                                            body.GetValue("name") + "," +
                                            body.GetValue("type_name") + "," +
                                            body.GetValue("distance_to_arrival").ToString() + ",\"$" +
                                            value.ToString("N0") + "\"," +
                                            terraforming + "," +
                                            view;
                                Console.WriteLine(line);
                                CSV.WriteLine(line);
                            }
                        }
                    }
                    Console.WriteLine(total);
                    CSV.WriteLine(("\"$" + total.ToString("N0") + "\",,,,,,,*"));
                }
            }
        }
    }
}
