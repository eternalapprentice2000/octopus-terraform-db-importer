using Newtonsoft.Json;

namespace Octopus.Terraform.VariableSet.Importer.Pocos
{
    public class LibrarySetStateInstanceAttributesTemplate {
        
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string? Label {get; set;}

        [JsonProperty("help_text", NullValueHandling = NullValueHandling.Ignore)]
        public string? HelpText {get; set;}

        [JsonProperty("default_value", NullValueHandling = NullValueHandling.Ignore)]
        public string? DefaultValue {get; set;}

        [JsonProperty("display_settings", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string>? DisplaySettings {get; set;}

        public LibrarySetStateInstanceAttributesTemplate(LibrarySetTemplate template){
            if (template == null) throw new Exception("Unable to process, template value cannot be null");

            this.Id = template.Id;
            this.Name = template.Name;
            this.Label = template.Label == String.Empty ? null : template.Label;
            this.HelpText = template.HelpText;
            this.DefaultValue = template.DefaultValue;
            this.DisplaySettings = template.DisplaySettings?.Count == 0 ? null : template.DisplaySettings;
        }
    }



    public class LibrarySetStateInstanceAttributes {
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description {get; set;}

        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("space_id")]
        public string SpaceId {get; set;}

        [JsonProperty("template")]
        public List<LibrarySetStateInstanceAttributesTemplate> Templates {get; set;}

        public LibrarySetStateInstanceAttributes(LibrarySet librarySet){
            this.Description = librarySet.LibrarySetJson?.Description;
            this.Id = librarySet.LibrarySetId;
            this.Name = librarySet.LibrarySetName;
            this.SpaceId = librarySet.LibrarySetSpaceId;

            if (librarySet.LibrarySetJson?.Templates != null){
                this.Templates = librarySet.LibrarySetJson.Templates.Select(x => new LibrarySetStateInstanceAttributesTemplate(x)).ToList();
            } else {
                this.Templates = new List<LibrarySetStateInstanceAttributesTemplate>();
            }

            

        }


    }

    public class LibrarySetStateInstance {

        [JsonProperty("schema_version")]
        public int SchemaVersion {
            get{
                return 0;
            }
        }

        [JsonProperty("attributes")]
        public LibrarySetStateInstanceAttributes Attributes;

        public LibrarySetStateInstance(LibrarySet librarySet){
            this.Attributes = new LibrarySetStateInstanceAttributes(librarySet);

        }

    }

    public class LibrarySetState {

        private readonly string? _module;

        [JsonProperty("module")]
        public string? Module {
            get{
                return $"module.{_module}";
            }
        }

        [JsonProperty("mode")]
        public string? Mode {
            get{
                return "managed";
            }
        }

        [JsonProperty("type")]
        public string? Type {
            get{
                return "octopusdeploy_library_variable_set";
            }
        }

        [JsonProperty("name")]
        public string? Name {
            get{
                return "variable_set";
            }
        }

        [JsonProperty("provider")]
        public string? Provider{
            get{
                return "provider[\"registry.terraform.io/octopusdeploylabs/octopusdeploy\"]";
            }
        }

        [JsonProperty("instances")]
        public List<LibrarySetStateInstance> Instances;

        public LibrarySetState(LibrarySet librarySet, string moduleName){
            this._module = moduleName;
            this.Instances = new List<LibrarySetStateInstance>{
                new LibrarySetStateInstance(librarySet)
            };



        }

        public override string ToString(){
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}