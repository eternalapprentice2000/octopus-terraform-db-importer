using Octopus.Terraform.VariableSet.Importer.SqlResults;
using Newtonsoft.Json;

namespace Octopus.Terraform.VariableSet.Importer.Pocos
{
    public class TagSetJson {
        public string? Description { get; set; }
        public List<TagSetJsonTags>? Tags { get; set; }
    
    }

    public class TagSetJsonTags { 
        public string? Id { get; set; }
        public string? CanonicalName { get; set; }
    }

    public class TagSets
    {
        public Dictionary<string, string?> Tags { get; set; }   
        public TagSets(List<TagSetResult> results) {

            Tags = new Dictionary<string, string?>();

            foreach (var result in results) {
                var json = JsonConvert.DeserializeObject<TagSetJson>(result?.JSON ?? "[]");

                if (json?.Tags != null && json.Tags.Count > 0)
                {
                    foreach (var tag in json.Tags)
                    {
                        if (tag.Id != null)
                        {
                            Tags.Add(tag.Id.ToLower(), tag.CanonicalName);
                        }
                    }
                }
            }
        }
    }
}
