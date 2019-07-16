using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GoogleGPSConverter
{
    struct location
    {
        public long timestamp;
        public double latitude;
        public double longitude;
        public double accuracy;

        public override string ToString()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan unix = TimeSpan.FromSeconds(timestamp);
            return $"{{{epoch.Add(unix)}; {latitude}°N; {longitude}°E; {accuracy}%}},\n";
        }

        public bool valid()
        {
            return timestamp != 0 && latitude != 0 && longitude != 0 && accuracy != 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Input: ");
            string infile = Console.ReadLine().Replace("\"", "");
            Console.Write("Output: ");
            string outfile = Console.ReadLine().Replace("\"", "");

            JsonTextReader r = new JsonTextReader(File.OpenText(infile));
            List<location> locs = new List<location>();

            while (r.Read())
            {
                if (r.TokenType == JsonToken.StartObject)
                {
                    location l = new location();
                    while (r.Read() && r.TokenType != JsonToken.EndObject)
                    {
                        string vn = r.Value as string;
                        if (r.TokenType == JsonToken.PropertyName)
                        {
                            if (vn == "timestampMs") l.timestamp = long.Parse(r.ReadAsString()) / 1000;
                            else if (vn == "latitudeE7") l.latitude = r.ReadAsInt32().Value / 1e7;
                            else if (vn == "longitudeE7") l.longitude = r.ReadAsInt32().Value / 1e7;
                            else if (vn == "accuracy") l.accuracy = percent(r.ReadAsInt32().Value);
                        }
                        else if (r.TokenType == JsonToken.StartObject)
                        {
                            while (r.Read() && r.TokenType != JsonToken.EndObject) ;
                        }
                    }
                    if(l.valid()) locs.Add(l);
                }
            }

            r.Close();

            FileStream fs = File.Open(outfile, FileMode.Create, FileAccess.Write);
            foreach(location l in locs)
            {
                byte[] b = Encoding.UTF8.GetBytes(l.ToString());
                fs.Write(b, 0, b.Length);
            }
            fs.Close();
        }

        static double percent(int i)
        {
            if (i < 1e2)
                return i;
            else if (i < 1e3)
                return i / 1e1;
            else if (i < 1e4)
                return i / 1e2;
            else if (i < 1e5)
                return i / 1e3;
            else if (i < 1e6)
                return i / 1e4;
            else
                return 0;
        }
    }
}
