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

            switch (action.ToLower())
            {
                case "import-library-set":
                    if (args.Length != 4) throw new Exception("incorrect number of parameters... usage `import-libary-set <set-id> <tf module name> <out file>`");
                    _ = int.TryParse(args[1], out int _id);

                    ImportLibrarySet(_id, args[2], args[3]);
                    break;
                default:
                    break;
            }
        }

        private static void ImportLibrarySet(int id, string moduleName, string outFile) { 
            Console.WriteLine($"Importing Library Set #{id}");

            var variableSetId = $"LibraryVariableSets-{id}";

            var result = sqlBusiness.GetLibrarySetById(variableSetId);
            var tagSets = sqlBusiness.GetTagSets();
            var envs = sqlBusiness.GetEnvironments();

            if (result == null) throw new Exception("libraryset result is null");

            // use template
            var template = new LibraryVariableSetsModule(result, tagSets, envs, moduleName);
            var content = template.TransformText();

            File.WriteAllText(outFile, content);

            // write the state file segment for the library sets
            var stateInfo = new LibrarySetState(result, moduleName);
            File.WriteAllText($"{outFile}.libraryset.json", stateInfo.ToString());

            // write the state for the variables for the library set
            var variableStateInfo = new VariableState(result, moduleName, tagSets);
            File.WriteAllText($"{outFile}.variables.json", variableStateInfo.ToString());
        }
    }
}