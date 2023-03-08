using Newtonsoft.Json;
using Octopus.Terraform.VariableSet.Importer.SqlResults;

namespace Octopus.Terraform.VariableSet.Importer.Pocos
{
    public class LibrarySet
    {
        public LibrarySet() {
            
        }

        public LibrarySet(LibrarySetResult sqlResult)
        {
            LibrarySetId = sqlResult.LibrarySetId;
            LibrarySetName = sqlResult.LibrarySetName;
            VariableSetId = sqlResult.VariableSetId;

            LibrarySetJson = JsonConvert.DeserializeObject<LibrarySetJson?>(sqlResult.LibrarySetJson ?? "{}");
            VariableSetJson = JsonConvert.DeserializeObject<VariableSetJson?>(sqlResult.VariableSetJson ?? "{}");
        }

        public string? LibrarySetId { get; set; }
        public string? LibrarySetName { get; set; }
        public LibrarySetJson? LibrarySetJson { get; set; }
        public string? VariableSetId { get; set; }
        public VariableSetJson? VariableSetJson { get; set; }
    }

    public class LibrarySetJson
    {
        public string? Description { get; set; }
        public List<LibrarySetTemplate>? Templates { get; set; }

    }

    public class LibrarySetTemplate
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Label { get; set; }
        public string? HelpText { get; set; }
        public string? DefaultValue { get; set; }
        public Dictionary<string, string>? DisplaySettings { get; set; }
    }

    public class VariableSetJson
    {
        public List<VariableSetVariable>? Variables { get; set; }
    }
    public class VariableSetVariable
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }

        public VariableSetVariableScope? Scope { get; set; }

    }

    public class VariableSetVariableScope
    {
        public string[]? Environment { get; set; }
        public string[]? TenantTag { get; set; }
    }
}
