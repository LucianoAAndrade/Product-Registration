using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o login")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Informe o Key")]
        public string Key { get; set; }
        [Required(ErrorMessage = "Informe o nome")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Informe o e-mail")]
        public string Email { get; set; }

        public static UserModel ValidateUser(string login, string Key)
        {
            UserModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from usuario where login=@login and Key=@Key";

                    command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
                    command.Parameters.Add("@Key", MySqlDbType.VarChar).Value = CriptoHelper.HashMD5(Key);

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new UserModel
                        {
                            Id = (int)reader["id"],
                            Login = (string)reader["login"],
                            Key = (string)reader["Key"],
                            Name = (string)reader["nome"],
                            Email = (string)reader["email"]
                        };
                    }
                }
            }

            return ret;
        }

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
                    command.CommandText = "select count(*) from usuario";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<UserModel> RecoverList(int page = -1, int pageSize = -1, string order = "")
        {
            var ret = new List<UserModel>();

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    var pos = (page - 1) * pageSize;

                    command.Connection = Connection;

                    if (page == -1 || pageSize == -1)
                    {
                        command.CommandText =
                            "select *" +
                            "from usuario" +
                            " order by " + (!string.IsNullOrEmpty(order) ? order : "nome");
                    }
                    else
                    {
                        command.CommandText = string.Format(
                            "select *" +
                            " from usuario" +
                            " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                            " offset {0} rows fetch next {1} rows only",
                            pos > 0 ? pos - 1 : 0, pageSize);
                    }

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new UserModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Login = (string)reader["login"],
                            Email = (string)reader["email"]
                        });
                    }
                }
            }

            return ret;
        }

        public static UserModel RecoverById(int id)
        {
            UserModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from usuario where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new UserModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Login = (string)reader["login"],
                            Email = (string)reader["email"]
                        };
                    }
                }
            }

            return ret;
        }

        public static UserModel RecuperarPeloLogin(string login)
        {
            UserModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from usuario where (login = @login)";

                    command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new UserModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Login = (string)reader["login"],
                            Email = (string)reader["email"]
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
                        command.CommandText = "delete from usuario where (id = @id)";

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
                        command.CommandText = "insert into usuario (nome, email, login, Key) values (@nome, @email, @login, @Key); select convert(int, scope_identity())";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@email", MySqlDbType.VarChar).Value = this.Email;
                        command.Parameters.Add("@login", MySqlDbType.VarChar).Value = this.Login;
                        command.Parameters.Add("@Key", MySqlDbType.VarChar).Value = CriptoHelper.HashMD5(this.Key);

                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText =
                            "update usuario set nome=@nome, email=@email, login=@login" +
                            (!string.IsNullOrEmpty(this.Key) ? ", Key=@Key" : "") +
                            " where id = @id";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@email", MySqlDbType.VarChar).Value = this.Email;
                        command.Parameters.Add("@login", MySqlDbType.VarChar).Value = this.Login;

                        if (!string.IsNullOrEmpty(this.Key))
                        {
                            command.Parameters.Add("@Key", MySqlDbType.VarChar).Value = CriptoHelper.HashMD5(this.Key);
                        }

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

        public string RecuperarStringNamePerfis()
        {
            var ret = string.Empty;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = string.Format(
                        "select p.nome " +
                        "from perfil_usuario pu, perfil p " +
                        "where (pu.id_usuario = @id_usuario) and (pu.id_perfil = p.id) and (p.ativo = 1)");

                    command.Parameters.Add("@id_usuario", MySqlDbType.Int32).Value = this.Id;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret += (ret != string.Empty ? ";" : string.Empty) + (string)reader["nome"];
                    }
                }
            }

            return ret;
        }

        public bool ValidarKeyAtual(string KeyAtual)
        {
            var ret = false;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;

                    command.CommandText = "select count(*) from usuario where Key = @KeyAtual and id = @id";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = this.Id;
                    command.Parameters.Add("@KeyAtual", MySqlDbType.VarChar).Value = CriptoHelper.HashMD5(KeyAtual);

                    ret = ((int)command.ExecuteScalar() > 0);
                }
            }

            return ret;
        }

        public bool AlterarKey(string novaKey)
        {
            var ret = false;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;

                    command.CommandText = "update usuario set Key = @Key where id = @id";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = this.Id;
                    command.Parameters.Add("@Key", MySqlDbType.VarChar).Value = CriptoHelper.HashMD5(novaKey);

                    ret = (command.ExecuteNonQuery() > 0);
                }
            }

            return ret;
        }
    }
}