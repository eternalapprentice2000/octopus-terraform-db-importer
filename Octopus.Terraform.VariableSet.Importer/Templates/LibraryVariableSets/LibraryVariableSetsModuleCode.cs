using Octopus.Terraform.VariableSet.Importer.Pocos;
using OctopusTenantCreator.Common;
using System.Collections;

namespace Octopus.Terraform.VariableSet.Importer.Templates.LibraryVariableSets
{
    class VariableComparer : IComparer<VariableSetVariable>
    {
        public int Compare(VariableSetVariable? x, VariableSetVariable? y)
        {
            return (new CaseInsensitiveComparer()).Compare(x?.Name, y?.Name);
        }
    }

    partial class LibraryVariableSetsModule
    {
        private readonly LibrarySet _mData;
        private readonly string _moduleName;
        private readonly TagSets _tagSetData;
        private readonly Environments _environments;
        private readonly string? _databaseMasterKey;
        private readonly Cryptography _cryptography;
        const string ignore = "##IGNORE##";
        public LibraryVariableSetsModule(LibrarySet mData, TagSets tagSetData, Environments environments, string? databaseMasterKey, string moduleName)
        {
            _mData = mData;
            _tagSetData = tagSetData;
            _environments = environments;
            _databaseMasterKey = databaseMasterKey;
            _cryptography = new Cryptography(_databaseMasterKey);
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

                } else
                {
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
        string _renderScopeEnvironments(string[]? environments)
        {
            if (environments == null) return ignore;

            var results = new List<string>();

            results.Add(_renderText("environments = [", 4));

            foreach (var environment in environments) {
                var envName = "";

                if (_environments.Environment != null && _environments.Environment.ContainsKey(environment.ToLower())){
                    envName = _environments.Environment[environment.ToLower()];
                }


                results.Add(_renderText($"\"{environment}\", ## {envName}", 5));
            }

            results.Add(_renderText("]", 4));


            _ = results.RemoveAll(x => x == ignore);
            return String.Join("\n", results);
        }

        string _renderScopeTenantTags(string[]? tenantTags) {
            if (tenantTags == null) return ignore;

            var results = new List<string>();

            results.Add(_renderText("tenant_tags = [", 4));

            foreach (var tag in tenantTags) {

                if (_tagSetData.Tags.ContainsKey(tag.ToLower()))
                {
                    var canonicalTag = _tagSetData.Tags[tag.ToLower()];
                    results.Add(_renderText($"\"{canonicalTag}\",", 5));
                }
            }

            results.Add(_renderText("]", 4));

            _ = results.RemoveAll(x => x == ignore);
            return String.Join("\n", results);
        }

        string _renderScope(VariableSetVariableScope? scope) {
            if (scope == null) return ignore;
            var results = new List<string>();

            results.Add(_renderText("scope = {", 3));

            results.Add(_renderScopeTenantTags(scope.TenantTag));
            results.Add(_renderScopeEnvironments(scope.Environment));

            results.Add(_renderText("}", 3));
            _ = results.RemoveAll(x => x == ignore);
            return string.Join("\n", results);
        }

        string _renderVariables() {
            var variables = this._mData.VariableSetJson?.Variables;

            if (variables == null) return String.Empty;

            variables.Sort(new VariableComparer());

            var results = new List<string>();

            for (var i = 0; i <= variables.Count - 1; i++) {
                var variable = variables[i];

                if (variable != null)
                {
                    results.Add(_renderText("{", 2));
                    results.Add(_renderItem("name", variable.Name, 3));
                    results.Add(_renderItem("description", variable.Description, 3));
                    results.Add(_renderItem("type", variable.Type, 3));

                    if (variable.Type == "Sensitive")
                    {
                        // get actual value
                        var actualValue = "SENSITIVE";

                        if (variable.Value != null && !string.IsNullOrWhiteSpace(_databaseMasterKey)){
                            actualValue = _cryptography.DecryptSensitiveVariable(variable.Value);
                        }

                        results.Add(_renderItem("value", "SENSITIVE VALUE", 3, $"DANGER >> {actualValue}"));
                    }
                    else 
                    {
                        results.Add(_renderItem("value", variable.Value, 3));
                    }

                    results.Add(_renderScope(variable.Scope));
                   

                    if (i != variables.Count - 1)
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
    }
}