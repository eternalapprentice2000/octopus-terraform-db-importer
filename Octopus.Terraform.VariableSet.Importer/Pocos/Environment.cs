using Octopus.Terraform.VariableSet.Importer.SqlResults;

namespace Octopus.Terraform.VariableSet.Importer.Pocos
{
    public class Environments
    {
        public Dictionary<string, string?>? Environment { get; set; }

        public Environments(List<EnvironmentResult> results) {
            Environment = new Dictionary<string, string?>();

            foreach(var result in results)
            {
                if (result?.Id != null)
                {
                    Environment.Add(result.Id.ToLower(), result.Name);
                }
            }
        }
    }
}
