using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class CityModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        [MaxLength(30, ErrorMessage = "The name can be a maximum of 30 characters.")]
        public string Name { get; set; }

        public bool Active { get; set; }

        [Required(ErrorMessage = "Select coutries.")]
        public int IdCountries { get; set; }

        [Required(ErrorMessage = "Select state.")]
        public int IdState { get; set; }

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
                    command.CommandText = "select count(*) from cidade";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<CityModel> RecoverList(int page = 0, int pageSize = 0, string filter = "", int idState = 0, string order = "")
        {
            var ret = new List<CityModel>();

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
                        filtroWhere = string.Format(" (lower(c.nome) like '%{0}%') and", filter.ToLower());
                    }

                    if (idState > 0)
                    {
                        filtroWhere += string.Format(" (id_estado = {0}) and", idState);
                    }

                    var pagetion = "";
                    if (page > 0 && pageSize > 0)
                    {
                        pagetion = string.Format(" offset {0} rows fetch next {1} rows only",
                            pos > 0 ? pos - 1 : 0, pageSize);
                    }

                    command.Connection = Connection;
                    command.CommandText =
                        "select c.*, e.id_pais" +
                        " from cidade c, estado e" +
                        " where" +
                        filtroWhere +
                        " (c.id_estado = e.id)" +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "c.nome") +
                        pagetion;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new CityModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            IdState = (int)reader["id_estado"],
                            IdCountries = (int)reader["id_pais"],
                            Active = (bool)reader["ativo"]
                        });
                    }
                }
            }

            return ret;
        }

        public static CityModel RecoverById(int id)
        {
            CityModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select c.*, e.id_pais from cidade c, estado e where (c.id = @id) and (c.id_estado = e.id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new CityModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            IdState = (int)reader["id_estado"],
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
                        command.CommandText = "delete from cidade where (id = @id)";

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
                        command.CommandText = "insert into cidade (nome, id_estado, ativo) values (@nome, @id_estado, @ativo); select convert(int, scope_identity())";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@id_estado", MySqlDbType.Int32).Value = this.IdState;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);

                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText = "update cidade set nome=@nome, id_estado=@id_estado, ativo=@ativo where id = @id";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@id_estado", MySqlDbType.Int32).Value = this.IdState;
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