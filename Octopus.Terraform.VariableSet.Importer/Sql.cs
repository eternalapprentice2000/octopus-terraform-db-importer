using KG.System.Data.SqlClient.Extensions.ReaderWrapper;
using Octopus.Terraform.VariableSet.Importer.SqlResults;
using System.Data.SqlClient;

namespace Octopus.Terraform.VariableSet.Importer
{
    public class Sql
    {
        const string connString = "Server=localhost,8433;Database=octopus_default;User Id=sa;Password=P@ssW0rd!;";

        public List<EnvironmentResult>? GetAllEnvironments() {
            var result = new List<EnvironmentResult>();

            using (var conn = new SqlConnection(connString))
            {
                var sql = $@"
                    SELECT  [Id]
	                    ,   [Name]
                        ,   [JSON]
                    FROM    [octopus_default].[dbo].[DeploymentEnvironment]";
        conn.Open();

                using var cmd = new SqlCommand(sql, conn);
                using var reader = new DataReaderWrapper(cmd.ExecuteReader(), "");
                while (reader.Read())
                {
                    result.Add(new EnvironmentResult
                    {
                        Id = reader.GetString("Id"),
                        Name = reader.GetString("Name")
                    });
                }

                conn.Close();
            }

            return result.Count == 0 ? null : result;
        }

        public List<TagSetResult>? GetAllTagSets()
        {
            var result = new List<TagSetResult>();

            using (var conn = new SqlConnection(connString))
            {
                var sql = $@"
                    SELECT  [Id]
                        ,   [Name]
                        ,   [SortOrder]
                        ,   [JSON]
                    FROM [octopus_default].[dbo].[TagSet]";
                conn.Open();

                using var cmd = new SqlCommand(sql, conn);
                using var reader = new DataReaderWrapper(cmd.ExecuteReader(), "");
                while (reader.Read())
                {
                    result.Add(new TagSetResult
                    {
                        Id = reader.GetString("Id"),
                        Name = reader.GetString("Name"),
                        SortOrder = reader.GetInt32NotNull("SortOrder"),
                        JSON = reader.GetString("JSON")
                    });
                }

                conn.Close();
            }

            return result.Count == 0 ? null : result;
        }

        public LibrarySetResult? GetLibrarySetById(string id)
        {
            var returnValue = null as LibrarySetResult;

            using (var conn = new SqlConnection(connString))
            {
                var sql = $@"
                    SELECT 
		                    LVS.[Id]        as LibrarySetId
	                    ,   LVS.[Name]      as LibrarySetName
                        ,   LVS.[JSON]      as LibrarySetJson
                        ,   LVS.[SpaceId]   as LibrarySetSpaceId
	                    ,   VS.[Id]         as VariableSetId
	                    ,   VS.[JSON]       as VariableSetJson
                    FROM	[octopus_default].[dbo].[LibraryVariableSet] as LVS
                    join	dbo.VariableSet as VS 
	                    on		LVS.VariableSetId = VS.Id
                    Where LVS.Id = '{id}'";
                conn.Open();

                using var cmd = new SqlCommand(sql, conn);
                using var reader = new DataReaderWrapper(cmd.ExecuteReader(), "");
                while (reader.Read())
                {
                    returnValue = new LibrarySetResult
                    {
                        LibrarySetId        = reader.GetString("LibrarySetId"),
                        LibrarySetName      = reader.GetString("LibrarySetName"),
                        LibrarySetJson      = reader.GetString("LibrarySetJson"),
                        LibrarySetSpaceId   = reader.GetString("LibrarySetSpaceId"),
                        VariableSetId       = reader.GetString("VariableSetiD"),
                        VariableSetJson     = reader.GetString("VariableSetJson")

                    };
                }

                conn.Close();
            }

            return returnValue;
        }
    }
}
