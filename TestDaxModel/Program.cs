﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using Newtonsoft.Json;
using System.IO.Packaging;
using System.IO;
using Dax.Vpax.Tools;

// TODO
// - Import from DMV 1100 (check for missing attributes?)
namespace TestDaxModel
{
    class Program
    {


        static void Main(string[] args)
        {
            //
            // Retrieve DAX model from database connection
            //
            // String connection for Power Pivot
            // const string serverName = @"http://localhost:9000/xmla";
            // const string databaseName = "Microsoft_SQLServer_AnalysisServices";

            // const string serverName = @"localhost\tab19";
            // const string databaseName = "Adventure Works";
            // const string databaseName = "Adventure Works 2012 Tabular";
            // const string databaseName = "EnterpriseBI";
            const string serverName = "localhost:53382";
            const string databaseName = "1d83feec-d09a-4766-980e-757adb5690ea";
            const string pathOutput = @"c:\temp\";

            Console.WriteLine("Getting model {0}:{1}", serverName, databaseName);
            var database = Dax.Metadata.Extractor.TomExtractor.GetDatabase(serverName, databaseName);
            var daxModel = Dax.Metadata.Extractor.TomExtractor.GetDaxModel(serverName, databaseName, "TestDaxModel", "0.2");

            //
            // Test serialization of Dax.Model in JSON file
            //
            // ExportModelJSON(pathOutput, m);

            Console.WriteLine("Exporting to VertiPaq Analyzer View");

            // 
            // Create VertiPaq Analyzer views
            //
            Dax.ViewVpaExport.Model viewVpa = new Dax.ViewVpaExport.Model(daxModel);

            // Save JSON file
            // ExportJSON(pathOutput, export);
            Console.WriteLine($"   Table Count : {viewVpa.Tables.Count()}");
            Console.WriteLine($"   Column Count: {viewVpa.Columns.Count()}");
            string filename = pathOutput + databaseName + ".vpax";
            Console.WriteLine("Saving {0}...", filename);

            // Save VPAX file
            // old internal version ExportVPAX(filename, daxModel, export);
            VpaxTools.ExportVpax(filename, daxModel, viewVpa, database);
            Console.WriteLine("File saved.");
            // ImportExport();

            Console.WriteLine("=================");
            Console.WriteLine($"Loading {filename}...");
            
            var content = VpaxTools.ImportVpax(filename);
            // var view2 = new Dax.ViewVpaExport.Model(content.DaxModel);
            Console.WriteLine($"   Table Count : {viewVpa.Tables.Count()}");
            Console.WriteLine($"   Column Count: {viewVpa.Columns.Count()}");
            

        }

        private static void ImportExport()
        {
            string filename = @"c:\temp\AdventureWorks.vpax";
            string fileout = @"c:\temp\export.vpax";
            var content = VpaxTools.ImportVpax(filename);
            VpaxTools.ExportVpax(fileout, content.DaxModel, content.ViewVpa, content.TomDatabase);
        }
        /// <summary>
        /// Export the Dax.Model in JSON format
        /// </summary>
        /// <param name="pathOutput"></param>
        /// <param name="m"></param>
        private static void ExportModelJSON(string pathOutput, Dax.Metadata.Model m)
        {
            var json = JsonConvert.SerializeObject(
                m,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                });
            System.IO.File.WriteAllText(pathOutput + "model.json", json);
            // Console.WriteLine(json);
        }

        /// <summary>
        /// Export VertiPaq Analyzer JSON format (just for test, the same file is embedded in VPAX)
        /// </summary>
        /// <param name="pathOutput"></param>
        /// <param name="export"></param>
        private static void ExportJSON(string pathOutput, Dax.ViewVpaExport.Model export)
        {
            var json = JsonConvert.SerializeObject(
                export,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                });
            System.IO.File.WriteAllText(pathOutput + "export.json", json);
            // Console.WriteLine(json);
        }
        /*
        /// <summary>
        /// Export to VertiPaq Analyzer (VPAX) file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="pathOutput"></param>
        /// <param name="export"></param>
        private static void ExportVPAX(string path, Dax.Model.Model model, Dax.ViewVpaExport.Model export)
        {
            Uri uriModel = PackUriHelper.CreatePartUri(new Uri("DaxModel.json", UriKind.Relative));
            Uri uriModelVpa = PackUriHelper.CreatePartUri(new Uri("DaxModelVpa.json", UriKind.Relative));
            using (Package package = Package.Open(path, FileMode.Create))
            {
                using (TextWriter tw = new StreamWriter(package.CreatePart(uriModel, "application/json", CompressionOption.Maximum).GetStream(), Encoding.UTF8))
                {
                    tw.Write(
                        JsonConvert.SerializeObject(
                            model,
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.All,
                                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                            }
                        )
                    );
                    tw.Close();
                }
                using (TextWriter tw = new StreamWriter(package.CreatePart(uriModelVpa, "application/json", CompressionOption.Maximum).GetStream(), Encoding.UTF8))
                {
                    tw.Write(JsonConvert.SerializeObject(export, Formatting.Indented));
                    tw.Close();
                }
                package.Close();
            }
        }
        */
        /// <summary>
        /// Dump internal structure for permissions
        /// </summary>
        /// <param name="m"></param>
        private static void DumpPermissions(Dax.Metadata.Model m)
        {
            Console.WriteLine("------------------------");
            foreach (var t in m.Tables)
            {
                Console.WriteLine("---Table={0}", t.TableName);
                foreach (var tp in t.GetTablePermissions())
                {
                    Console.WriteLine("Role {0} = {1} ", tp.Role.RoleName.Name, tp.FilterExpression?.Expression);
                }
            }
        }

        /// <summary>
        /// Dump internal structure for Relationships
        /// </summary>
        /// <param name="m"></param>
        private static void DumpRelationships(Dax.Metadata.Model m)
        {
            Console.WriteLine("------------------------");
            foreach (var t in m.Tables)
            {
                Console.WriteLine("---Table={0}", t.TableName);
                foreach (var r in t.GetRelationshipsTo())
                {
                    Console.WriteLine(
                        "{0}[{1}] ==> {2}[{3}]",
                        r.FromColumn.Table.TableName,
                        r.FromColumn.ColumnName,
                        r.ToColumn.Table.TableName,
                        r.ToColumn.ColumnName
                    );
                }
                foreach (var r in t.GetRelationshipsFrom())
                {
                    Console.WriteLine(
                        "{0}[{1}] ==> {2}[{3}]",
                        r.FromColumn.Table.TableName,
                        r.FromColumn.ColumnName,
                        r.ToColumn.Table.TableName,
                        r.ToColumn.ColumnName
                    );
                }
            }
        }

        /// <summary>
        /// Dump internal model structure 
        /// </summary>
        static void SimpleDump()
        {
            Dax.Metadata.Model m = new Dax.Metadata.Model();
            Dax.Metadata.Table tA = new Dax.Metadata.Table(m)
            {
                TableName = new Dax.Metadata.DaxName("A")
            };
            Dax.Metadata.Table tB = new Dax.Metadata.Table(m)
            {
                TableName = new Dax.Metadata.DaxName("B")
            };
            Dax.Metadata.Column ca1 = new Dax.Metadata.Column(tA)
            {
                ColumnName = new Dax.Metadata.DaxName("A_1")
            };
            Dax.Metadata.Column ca2 = new Dax.Metadata.Column(tA)
            {
                ColumnName = new Dax.Metadata.DaxName("A_2")
            };
            tA.Columns.Add(ca1);
            tA.Columns.Add(ca2);
            m.Tables.Add(tA);

            // Test serialization on JSON file
            var json = JsonConvert.SerializeObject(m, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
            System.IO.File.WriteAllText(@"C:\temp\model.json", json);
        }
    }


}
