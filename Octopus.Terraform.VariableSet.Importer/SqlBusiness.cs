using Octopus.Terraform.VariableSet.Importer.Pocos;

namespace Octopus.Terraform.VariableSet.Importer
{
    public class SqlBusiness
    {
        private readonly Sql sql = new();

        public LibrarySet? GetLibrarySetById(string id)
        {
            var sqlResult = sql.GetLibrarySetById(id);

            if (sqlResult == null)
            {
                throw new Exception("LibrarySetResult was null");
            }

            var result = new LibrarySet(sqlResult);

            return result;
        }

        public TagSets GetTagSets()
        {
            var sqlResult = sql.GetAllTagSets();
            if (sqlResult == null) throw new Exception("TagSets was null");

            var result = new TagSets(sqlResult);
            return result;
        }

        public Environments GetEnvironments() {
            var sqlResult = sql.GetAllEnvironments();

            if (sqlResult == null) throw new Exception("Environments result was null");

            var result = new Environments(sqlResult);
            return result;
        }
    }
}
