using Newtonsoft.Json;
using OctopusTenantCreator.Common;

namespace Octopus.Terraform.VariableSet.Importer.Pocos
{
    public class VariableSetInstanceAttributesScope{

        [JsonProperty("actions")]
        public List<string> Actions {get; set;}

        [JsonProperty("channels")]
        public List<string> Channels {get;set;}

        [JsonProperty("environments")]
        public List<string> Environments {get;set;}

        [JsonProperty("machines")]
        public List<string> Machines {get;set;}

        [JsonProperty("roles")]
        public List<string> Roles {get;set;}

        [JsonProperty("tenant_tags")]
        public List<string> TenantTags {get;set;}

        public VariableSetInstanceAttributesScope(VariableSetVariableScope? scope, TagSets tagSets){
            this.Actions = scope?.Actions?.ToList() ?? new List<string>();
            this.Channels = scope?.Channels?.ToList() ?? new List<string>();
            this.Environments = scope?.Environment?.ToList() ?? new List<string>();
            this.Machines = scope?.Machines?.ToList() ?? new List<string>();
            this.Roles = scope?.Roles?.ToList() ?? new List<string>();
            this.TenantTags = scope?.TenantTag?.Select(x => (tagSets.Tags.ContainsKey(x.ToLower()) ? tagSets.Tags[x.ToLower()] : x) ?? String.Empty).ToList() ?? new List<string>();
        }
    }


    public class VariableSetInstanceAttributes {

        [JsonProperty("description")]
        public string Description {get;set;}

        [JsonProperty("encrypted_value")]
        public string? EncryptedValue {get;set;}

        [JsonProperty("id")]
        public string Id {get;set;}

        [JsonProperty("is_editable")]
        public bool IsEditable {
            get{
                return true;
            }
        }

        [JsonProperty("is_sensitive")]
        public bool IsSensitive {
            get{
                return this.Type.ToLower() == "sensitive";
            }
        }

        [JsonProperty("key_fingerprint")]
        public string? KeyFingerprint{
            get{
                return null;
            }
        }

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("owner_id")]
        public string OwnerId {get;set;}

        [JsonProperty("pgp_key")]
        public string? PgpKey {
            get {
                return null;
            }
        }

        [JsonProperty("project_id")]
        public string? ProjectId {
            get {
                return null;
            }
        }

        [JsonProperty("prompt")]
        public List<string> Prompt{
            get{
                return new List<string>();
            }
        }

        [JsonProperty("scope")]
        public List<VariableSetInstanceAttributesScope> Scope {get; set;}



        [JsonProperty("type")]
        public string Type {get;set;}

        private string? _value {get;set;}

        [JsonProperty("value")]
        public string? Value {
            get {
                return this.IsSensitive ? null : this._value;
            }
        }

        [JsonProperty("sensitive_value")]
        public string? SensitiveValue {
            get {
                return this.IsSensitive ? this._value : null;
            }
        }

        public VariableSetInstanceAttributes(VariableSetVariable variableSetVariable, string librarySetOwnerId, TagSets tagSets){
            if (variableSetVariable != null){
                this.Description = variableSetVariable.Description ?? String.Empty;
                this.Id = variableSetVariable.Id;
                this.Name = variableSetVariable.Name;
                this.OwnerId = librarySetOwnerId;
                this.Scope = new List<VariableSetInstanceAttributesScope>{
                    new VariableSetInstanceAttributesScope(variableSetVariable.Scope, tagSets)
                };
            
                this.Type = variableSetVariable.Type ?? "String";
                this._value = Cryptography.DecryptSensitiveVariable(variableSetVariable.Value);
            }
        }
    }

    public class VariableSetInstance {
        
        [JsonProperty("index_key")]
        public string IndexKey {get; set;}

        [JsonProperty("schema_version")]
        public int SchemaVersion {
            get{
                return 0;
            }
        }

        [JsonProperty("attributes")]
        public VariableSetInstanceAttributes Attributes {get; set;}
        
        [JsonProperty("sensitive_attributes")]
        public List<string> SensitiveAttributes {get;set;}

        [JsonProperty("private")]
        public string Private {
            get {
                return Environment.GetEnvironmentVariable("PRIVATE_VARIABLE") ?? "";
            }
        }

        public VariableSetInstance (VariableSetVariable variable, string librarySetOwnerId, TagSets tagSets){
            this.IndexKey = $"{variable.Name}_{variable.Id}";
            this.SensitiveAttributes = new List<string>();
            this.Attributes = new VariableSetInstanceAttributes(variable, librarySetOwnerId, tagSets);
        }
    }

    public class VariableState {
        private string _moduleName { get; set; }

        [JsonProperty("module")]
        public string Module {
            get {
                return $"module.{_moduleName}";
            }
        }

        [JsonProperty("mode")]
        public string Mode {
            get{
                return "managed";
            }
        }

        [JsonProperty("type")]
        public string Type {
            get {
                return "octopusdeploy_variable";
            }
        }

        [JsonProperty("name")]
        public string Name {
            get{
                return "variables"; //hard coded into the terraform module
            }
        }

        [JsonProperty("provider")]
        public string Provider{
            get{
                return "provider[\"registry.terraform.io/octopusdeploylabs/octopusdeploy\"]";
            }
        }

        [JsonProperty("instances")]
        public List<VariableSetInstance> Instances {get;set;}

        public VariableState(LibrarySet librarySet, string moduleName, TagSets tagSets){
            this._moduleName = moduleName;
            this.Instances = new List<VariableSetInstance>();

            var sortedVariables = librarySet.VariableSetJson?.GetSortedVariables();

            if (sortedVariables != null){
                this.Instances = new List<VariableSetInstance>();
                foreach(var sortedVar in sortedVariables){
                    this.Instances.AddRange(
                        sortedVar.Value.Select(x => new VariableSetInstance(x, librarySet.LibrarySetId, tagSets))
                    );
                }
            } 
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}