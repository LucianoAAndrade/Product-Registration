using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class ProfileModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        public string Name { get; set; }

        public bool Active { get; set; }

        public List<UserModel> User { get; set; }

        public ProfileModel()
        {
            this.User = new List<UserModel>();
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
                    command.CommandText = "select count(*) from perfil";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<ProfileModel> RecoverList(int page, int pageSize, string order = "")
        {
            var ret = new List<ProfileModel>();

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    var pos = (page - 1) * pageSize;

                    command.Connection = Connection;
                    command.CommandText = string.Format(
                        "select *" +
                        " from perfil" +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                        " offset {0} rows fetch next {1} rows only",
                        pos > 0 ? pos - 1 : 0, pageSize);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new ProfileModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Active = (bool)reader["ativo"]
                        });
                    }
                }
            }

            return ret;
        }

        public void CarregarUser()
        {
            this.User.Clear();

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText =
                        "select u.* " +
                        "from perfil_usuario pu, usuario u " +
                        "where (pu.id_perfil = @id_perfil) and (pu.id_usuario = u.id)";

                    command.Parameters.Add("@id_perfil", MySqlDbType.Int32).Value = this.Id;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        this.User.Add(new UserModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Login = (string)reader["login"]
                        });
                    }
                }
            }
        }

        public static List<ProfileModel> RecoverListActives()
        {
            var ret = new List<ProfileModel>();

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = string.Format("select * from perfil where ativo=1 order by nome");
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new ProfileModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            Active = (bool)reader["ativo"]
                        });
                    }
                }
            }

            return ret;
        }

        public static ProfileModel RecoverById(int id)
        {
            ProfileModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from perfil where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new ProfileModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
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
                        command.CommandText = "delete from perfil where (id = @id)";

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

                using (var transaction = Connection.BeginTransaction())
                {
                    using (var command = new MySqlCommand())
                    {
                        command.Connection = Connection;
                        command.Transaction = transaction;

                        if (model == null)
                        {
                            command.CommandText = "insert into perfil (nome, ativo) values (@nome, @ativo); select convert(int, scope_identity())";

                            command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                            command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);

                            ret = (int)command.ExecuteScalar();
                            this.Id = ret;
                        }
                        else
                        {
                            command.CommandText = "update perfil set nome=@nome, ativo=@ativo where id = @id";

                            command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                            command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);
                            command.Parameters.Add("@id", MySqlDbType.Int32).Value = this.Id;

                            if (command.ExecuteNonQuery() > 0)
                            {
                                ret = this.Id;
                            }
                        }
                    }

                    if (this.User != null && this.User.Count > 0)
                    {
                        using (var commandExclusaoProfileUser = new MySqlCommand())
                        {
                            commandExclusaoProfileUser.Connection = Connection;
                            commandExclusaoProfileUser.Transaction = transaction;

                            commandExclusaoProfileUser.CommandText = "delete from perfil_usuario where (id_perfil = @id_perfil)";
                            commandExclusaoProfileUser.Parameters.Add("@id_perfil", MySqlDbType.Int32).Value = this.Id;

                            commandExclusaoProfileUser.ExecuteScalar();
                        }

                        if (this.User[0].Id != -1)
                        {
                            foreach (var usuario in this.User)
                            {
                                using (var usuarioInclusaoProfileUser = new MySqlCommand())
                                {
                                    usuarioInclusaoProfileUser.Connection = Connection;
                                    usuarioInclusaoProfileUser.Transaction = transaction;

                                    usuarioInclusaoProfileUser.CommandText = "insert into perfil_usuario (id_perfil, id_usuario) values (@id_perfil, @id_usuario)";
                                    usuarioInclusaoProfileUser.Parameters.Add("@id_perfil", MySqlDbType.Int32).Value = this.Id;
                                    usuarioInclusaoProfileUser.Parameters.Add("@id_usuario", MySqlDbType.Int32).Value = usuario.Id;

                                    usuarioInclusaoProfileUser.ExecuteScalar();
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }
            }

            return ret;
        }
    }
}