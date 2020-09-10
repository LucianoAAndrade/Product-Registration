using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class StateModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        [MaxLength(30, ErrorMessage = "The name can be a maximum of 30 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Fill the UF.")]
        [MaxLength(3, ErrorMessage = "UF must be 2 characters.")]
        public string UF { get; set; }

        public bool Active { get; set; }

        [Required(ErrorMessage = "Select country.")]
        public int IdCountries { get; set; }

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
                    command.CommandText = "select count(*) from estado";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<StateModel> RecoverList(int page = 0, int pageSize = 0, string filtro = "", int idCountries = 0, string order = "")
        {
            var ret = new List<StateModel>();

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    var pos = (page - 1) * pageSize;

                    var filtroWhere = "";
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        filtroWhere = string.Format(" where lower(nome) like '%{0}%'", filtro.ToLower());
                    }

                    if (idCountries > 0)
                    {
                        filtroWhere +=
                            (string.IsNullOrEmpty(filtroWhere) ? " where" : " and") +
                            string.Format(" id_pais = {0}", idCountries);
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
                        " from estado" +
                        filtroWhere +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                        pagecao;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new StateModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            UF = (string)reader["uf"],
                            IdCountries = (int)reader["id_pais"],
                            Active = (bool)reader["ativo"]
                        });
                    }
                }
            }

            return ret;
        }

        public static StateModel RecoverById(int id)
        {
            StateModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from estado where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new StateModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            UF = (string)reader["uf"],
                            IdCountries = (int)reader["id_pais"],
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
                        command.CommandText = "delete from estado where (id = @id)";

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
                        command.CommandText = "insert into estado (nome, uf, id_pais, ativo) values (@nome, @uf, @id_pais, @ativo); select convert(int, scope_identity())";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@uf", MySqlDbType.VarChar).Value = this.UF;
                        command.Parameters.Add("@id_pais", MySqlDbType.Int32).Value = this.IdCountries;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);

                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText = "update estado set nome=@nome, uf=@uf, id_pais=@id_pais, ativo=@ativo where id = @id";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@uf", MySqlDbType.VarChar).Value = this.UF;
                        command.Parameters.Add("@id_pais", MySqlDbType.Int32).Value = this.IdCountries;
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