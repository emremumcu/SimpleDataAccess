using SimpleDataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccessDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Configuration _configuration = ConfigurationManager.OpenMappedExeConfiguration
                (
                    new ExeConfigurationFileMap() { ExeConfigFilename = $@"data.config" },
                    ConfigurationUserLevel.None
                );

                ConnectionStringSettings connStr = _configuration.ConnectionStrings.ConnectionStrings["SQL"];

                DbManager db = new DbManager(DbProvider.MSSqlServer, connStr.ConnectionString);

                EntityCreator ec = new EntityCreator(db); 

                string s = ec.Create("Person", "Person");



                Stopwatch watch = Stopwatch.StartNew();

                List<Person> personList = db.SelectAll<Person>();

                watch.Stop();

                Console.WriteLine($"{personList.Count} rows loaded in { watch.ElapsedMilliseconds } miliseconds");




            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();

        }
    }
}
