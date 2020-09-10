using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class CountriesModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        [MaxLength(30, ErrorMessage = "The name can be a maximum of 30 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Fill in the international code.")]
        [MaxLength(3, ErrorMessage = "International code must be 3 characters.")]
        public string Code { get; set; }

        public bool Active { get; set; }

        public static int RecoverQuantity()
        {
            var ret = 0;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select count(*) from pais";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<CountriesModel> RecoverList(int page = 0, int pageSize = 0, string filter = "", string order = "")
        {
            var ret = new List<CountriesModel>();

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    var pos = (page - 1) * pageSize;

                    var filtroWhere = "";
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filtroWhere = string.Format(" where lower(nome) like '%{0}%'", filter.ToLower());
                    }

                    var pagecao = "";
                    if (page > 0 && pageSize > 0)
                    {
                        pagecao = string.Format(" offset {0} rows fetch next {1} rows only",
                            pos > 0 ? pos - 1 : 0, pageSize);
                    }

                    command.Connection = Connection;
                    command.CommandText =
                        "select *" +
                        " from pais" +
                        filtroWhere +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                        pagecao;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new CountriesModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Code = (string)reader["codigo"],
                            Active = (bool)reader["ativo"]
                        });
                    }
                }
            }

            return ret;
        }

        public static CountriesModel RecoverById(int id)
        {
            CountriesModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from pais where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new CountriesModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Code = (string)reader["codigo"],
                            Active = (bool)reader["ativo"]
                        };
                    }
                }
            }

            return ret;
        }

        public static bool ExcludeId(int id)
        {
            var ret = false;

            if (RecoverById(id) != null)
            {
                using (var Connection = new MySqlConnection())
                {
                    Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                    Connection.Open();
                    using (var command = new MySqlCommand())
                    {
                        command.Connection = Connection;
                        command.CommandText = "delete from pais where (id = @id)";

                        command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                        ret = (command.ExecuteNonQuery() > 0);
                    }
                }
            }

            return ret;
        }

        public int Save()
        {
            var ret = 0;

            var model = RecoverById(this.Id);

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;

                    if (model == null)
                    {
                        command.CommandText = "insert into pais (nome, codigo, ativo) values (@nome, @codigo, @ativo); select convert(int, scope_identity())";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@codigo", MySqlDbType.VarChar).Value = this.Code;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);

                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText = "update pais set nome=@nome, codigo=@codigo, ativo=@ativo where id = @id";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@codigo", MySqlDbType.VarChar).Value = this.Code;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);
                        command.Parameters.Add("@id", MySqlDbType.Int32).Value = this.Id;

                        if (command.ExecuteNonQuery() > 0)
                        {
                            ret = this.Id;
                        }
                    }
                }
            }

            return ret;
        }
    }
}