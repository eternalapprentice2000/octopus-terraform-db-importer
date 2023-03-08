using Octopus.Terraform.VariableSet.Importer;
using Octopus.Terraform.VariableSet.Importer.Templates.LibraryVariableSets;

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

        }
    }
}