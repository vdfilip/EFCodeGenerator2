using EFCodeGenerator.Logic;
using EFCodeGenerator.Repo;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EFCodeGenerator
{
    public class Program
    {
        public static void Main()
        {
            var exe = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(exe);

            if (folder != null)
            {
                var databaseName = ConfigurationManager.AppSettings["DatabaseName"];
                var outputPath = ConfigurationManager.AppSettings["OutputPath"];
                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionStrings"].ConnectionString;
                var repo = new MetaDataRepository(connectionString);

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                var sw = Stopwatch.StartNew();
                var generator = new Generator(databaseName, outputPath, repo) { EntityBaseClassName = "IEntity", PkPropertyName = "Id" };
                generator.Run();
                sw.Stop();

                Console.WriteLine($"Code generated in {sw.ElapsedMilliseconds} ms in folder: {outputPath}");
                Console.ReadKey();
            }
        }
    }
}