using Octopus.Terraform.VariableSet.Importer;
using Octopus.Terraform.VariableSet.Importer.Pocos;
using Octopus.Terraform.VariableSet.Importer.Templates.LibraryVariableSets;
using System;

namespace Octopus.Terraform.Importer
{
    internal class Program
    {
        internal static SqlBusiness sqlBusiness = new();

        static void Main(string[] args)
        {
            var action = args[0];
            var encryptionKey = Environment.GetEnvironmentVariable("OCTOPUS_MASTER_KEY");

            if (string.IsNullOrWhiteSpace(encryptionKey)){
                Console.WriteLine("Unable to locate database master key in Environment.  Secret Variable Decryption will be skipped");
                Console.WriteLine("To enable, please set OCTOPUS_MASTER_KEY environment variable to the octopus databaer master key");
            }
            

            switch (action.ToLower())
            {
                case "import-library-set":
                    if (args.Length != 4) throw new Exception("incorrect number of parameters... usage `import-libary-set <set-id> <tf module name> <out file>`");
                    _ = int.TryParse(args[1], out int _id);

                    ImportLibrarySet(_id, args[2], args[3], encryptionKey ?? null as string);
                    break;
                default:
                    break;
            }
        }

        private static void ImportLibrarySet(int id, string moduleName, string outFile, string? databaseMasterKey) { 
            Console.WriteLine($"Importing Library Set #{id}");

            var variableSetId = $"LibraryVariableSets-{id}";

            var result = sqlBusiness.GetLibrarySetById(variableSetId);
            var tagSets = sqlBusiness.GetTagSets();
            var envs = sqlBusiness.GetEnvironments();

            if (result == null) throw new Exception("libraryset result is null");

            // use template
            var template = new LibraryVariableSetsModule(result, tagSets, envs, databaseMasterKey, moduleName);
            var content = template.TransformText();

            File.WriteAllText(outFile, content);

            // write the state file segment for this
            var stateInfo = new LibrarySetState(result, moduleName);
            File.WriteAllText($"{outFile}.libraryset.json", stateInfo.ToString());




        }
    }
}