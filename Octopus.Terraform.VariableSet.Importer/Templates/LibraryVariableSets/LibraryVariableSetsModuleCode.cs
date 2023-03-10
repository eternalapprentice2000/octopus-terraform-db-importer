using Octopus.Terraform.VariableSet.Importer.Pocos;
using OctopusTenantCreator.Common;

namespace Octopus.Terraform.VariableSet.Importer.Templates.LibraryVariableSets
{

    partial class LibraryVariableSetsModule
    {
        private readonly LibrarySet _mData;
        private readonly string _moduleName;
        private readonly TagSets _tagSetData;
        private readonly Environments _environments;
        const string ignore = "##IGNORE##";
        public LibraryVariableSetsModule(LibrarySet mData, TagSets tagSetData, Environments environments, string moduleName)
        {
            _mData = mData;
            _tagSetData = tagSetData;
            _environments = environments;
            _moduleName = moduleName;
        }

        string _renderItem(string itemName, string? itemValue, int tabLevel = 2, string? comment = null)
        {
            if (!string.IsNullOrWhiteSpace(itemValue))
            {
                if (!string.IsNullOrWhiteSpace(comment)){
                    return $"{_getTab(tabLevel)}{itemName} = \"{itemValue.Replace(@"\", @"\\")}\" ## {comment}"; // in terraform '\' needs to be escaped in string values
                } else {
                    return $"{_getTab(tabLevel)}{itemName} = \"{itemValue.Replace(@"\", @"\\")}\""; // in terraform '\' needs to be escaped in string values
                }
                
            }
            return ignore;
        }

        string _getTab(int level)
        {
            return new string(' ', 4 * level);
        }

        string _renderDisplaySettings(Dictionary<string, string>? displaySettings)
        {
            if ((displaySettings?.Count ?? 0) == 0) return ignore;
            if ((displaySettings?.Keys == null || displaySettings.Keys.Count == 0)) return ignore;

            var results = new List<string>
            {
                $"{_getTab(3)}display_settings = {{"
            };

            foreach (var key in displaySettings.Keys)
            {
                // for Octopus.SelectOptions
                if (displaySettings.ContainsKey("Octopus.ControlType") && displaySettings["Octopus.ControlType"] == "Select" && key == "Octopus.SelectOptions")
                {
                    results.Add(_renderText($"\"{key}\" = join(\"\\n\",", 4));
                    results.Add(_renderText("[", 5));

                    var options = displaySettings[key].Split("\n");

                    //foreach (var option in options){
                    for (var i = 0; i <= (options.Length - 1); i++) { 
                        var option = options[i];

                        if (i != options.Length - 1){
                            results.Add(_renderText($"\"{option}\",", 6));
                        } else {

                            results.Add(_renderText($"\"{option}\"", 6));
                        }
                    }
                    results.Add(_renderText("]", 5));
                    results.Add(_renderText(")", 4));

                } else {
                    results.Add($"{_getTab(4)}\"{key}\" = \"{displaySettings[key].Replace("\n", "\\n")}\"");
                }
            }
            results.Add($"{_getTab(3)}}}");

            return String.Join("\n", results);

        }

        string _renderText(string text, int tabLevel = 0) {
            return $"{_getTab(tabLevel)}{text}";
        }

        string _renderTemplates() { 
            var results = new List<string>();

            for (var i = 0; i <= (this._mData.LibrarySetJson?.Templates?.Count -1); i++) {
                var template = this._mData.LibrarySetJson?.Templates?[i];

                if (template != null)
                {
                    results.Add(_renderText("{", 2));
                    results.Add(_renderItem("name", template.Name ?? "", 3));
                    results.Add(_renderItem("label", template.Label, 3));
                    results.Add(_renderItem("help_text", template.HelpText, 3));
                    results.Add(_renderItem("default_value", template.DefaultValue, 3));
                    results.Add(_renderDisplaySettings(template.DisplaySettings));

                    if (i != (this._mData.LibrarySetJson?.Templates?.Count - 1))
                    {
                        results.Add(_renderText("},", 2));
                    }
                    else
                    {
                        results.Add(_renderText("}", 2));
                    }
                }
            }

            _ = results.RemoveAll(x => x == ignore);
            return String.Join("\n", results);
        }
        string _renderScopeEnvironments(string[]? environments, int tabLevel)
        {
            if (environments == null) return ignore;

            var results = new List<string>();

            results.Add(_renderText("environments = [", tabLevel));

            foreach (var environment in environments) {
                var envName = "";

                if (_environments.Environment != null && _environments.Environment.ContainsKey(environment.ToLower())){
                    envName = _environments.Environment[environment.ToLower()];
                }


                results.Add(_renderText($"\"{environment}\", ## {envName}", tabLevel + 1));
            }

            results.Add(_renderText("]", tabLevel));


            _ = results.RemoveAll(x => x == ignore);
            return String.Join("\n", results);
        }

        string _renderScopeTenantTags(string[]? tenantTags, int tabLevel) {
            if (tenantTags == null) return ignore;

            var results = new List<string>();

            results.Add(_renderText("tenant_tags = [", tabLevel));

            foreach (var tag in tenantTags) {

                if (_tagSetData.Tags.ContainsKey(tag.ToLower()))
                {
                    var canonicalTag = _tagSetData.Tags[tag.ToLower()];
                    results.Add(_renderText($"\"{canonicalTag}\",", tabLevel + 1));
                }
            }

            results.Add(_renderText("]", tabLevel));

            _ = results.RemoveAll(x => x == ignore);
            return String.Join("\n", results);
        }

        string _renderScope(VariableSetVariableScope? scope, int tabLevel) {
            if (scope == null) return ignore;
            var results = new List<string>();

            results.Add(_renderText("scope = {", tabLevel));

            results.Add(_renderScopeTenantTags(scope.TenantTag, tabLevel + 1));
            results.Add(_renderScopeEnvironments(scope.Environment, tabLevel + 1));

            results.Add(_renderText("}", tabLevel));
            _ = results.RemoveAll(x => x == ignore);
            return string.Join("\n", results);
        }

        string _renderVariables() {
            var variables = this._mData.VariableSetJson?.GetSortedVariables();

            if (variables == null) return String.Empty;

            var results = new List<string>();

            foreach(var sortedVariable in variables){
                results.Add(_renderText("{", 2));
                results.Add(_renderItem("name", sortedVariable.Key, 3));
                results.Add(_renderText("values = [", 3));

                foreach(var variable in sortedVariable.Value){
                    results.Add(_renderText("{", 4));
                    results.Add(_renderItem("id", variable.Id, 5));
                    results.Add(_renderItem("description", variable.Description, 5));
                    results.Add(_renderItem("type", variable.Type, 5));

                    if (variable.Type == "Sensitive")
                    {
                        // get actual value
                        var actualValue = Cryptography.DecryptSensitiveVariable(variable.Value);
                        

                        results.Add(_renderItem("value", actualValue, 5, "DANGER!!!!!"));
                    }
                    else 
                    {
                        results.Add(_renderItem("value", variable.Value, 5));
                    }

                    results.Add(_renderScope(variable.Scope, 5));
                    results.Add(_renderText("},", 4));
                }

                results.Add(_renderText("]", 3));
                results.Add(_renderText("},", 2));
            }

            _ = results.RemoveAll(x => x == ignore);
            return String.Join("\n", results);
        
        }
    }
}