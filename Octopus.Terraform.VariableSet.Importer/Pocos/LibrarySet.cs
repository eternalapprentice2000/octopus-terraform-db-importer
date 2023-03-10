using Newtonsoft.Json;
using Octopus.Terraform.VariableSet.Importer.SqlResults;

namespace Octopus.Terraform.VariableSet.Importer.Pocos
{
    public class LibrarySet
    {
        public LibrarySet(LibrarySetResult sqlResult)
        {
            if (sqlResult == null || sqlResult.LibrarySetId == null || sqlResult.LibrarySetName == null || sqlResult.VariableSetId == null) {
                throw new Exception("unable to build library set, sql result contains null values");
            }

            LibrarySetId = sqlResult.LibrarySetId;
            LibrarySetName = sqlResult.LibrarySetName;
            VariableSetId = sqlResult.VariableSetId;
            LibrarySetSpaceId = sqlResult.LibrarySetSpaceId ?? "Spaces-1";

            LibrarySetJson = JsonConvert.DeserializeObject<LibrarySetJson?>(sqlResult.LibrarySetJson ?? "{}");
            VariableSetJson = JsonConvert.DeserializeObject<VariableSetJson?>(sqlResult.VariableSetJson ?? "{}");
        }

        public string LibrarySetId { get; set; }
        public string LibrarySetName { get; set; }
        public LibrarySetJson? LibrarySetJson { get; set; }
        public string VariableSetId { get; set; }
        public VariableSetJson? VariableSetJson { get; set; }
        public string LibrarySetSpaceId {get; set;}
    }

    public class LibrarySetJson
    {
        public string? Description { get; set; }
        public List<LibrarySetTemplate>? Templates { get; set; }

    }

    public class LibrarySetTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Label { get; set; }
        public string? HelpText { get; set; }
        public string? DefaultValue { get; set; }
        public Dictionary<string, string>? DisplaySettings { get; set; }
    }

    public class VariableSetJson
    {
        public List<VariableSetVariable>? Variables { get; set; }

        private Dictionary<string, List<VariableSetVariable>>? _sortedVariables {get; set;}

        public Dictionary<string, List<VariableSetVariable>>? GetSortedVariables() {
            if (this.Variables == null) return _sortedVariables;

            if (_sortedVariables == null) {
                _sortedVariables = new Dictionary<string, List<VariableSetVariable>>();

                foreach(var variable in this.Variables){
                    if (!_sortedVariables.ContainsKey(variable.Name)){
                        _sortedVariables.Add(variable.Name, new List<VariableSetVariable>());
                    } 
                    
                    // not possible to be null
                    _sortedVariables[variable.Name].Add(variable);
                }
            }

            return _sortedVariables;            
        }
    }
    public class VariableSetVariable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }

        public VariableSetVariableScope? Scope { get; set; }

    }

    public class VariableSetVariableScope
    {
        public string[]? Actions {get;set;}
        public string[]? Channels {get;set;}
        public string[]? Environment { get; set; }
        public string[]? Machines {get;set;}
        public string[]? Roles {get;set;}
        public string[]? TenantTag { get; set; }
    }
}
