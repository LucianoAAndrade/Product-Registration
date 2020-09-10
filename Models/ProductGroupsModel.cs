﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Web;

namespace ProductRegistration.Web.Models
{
    public class ProductGroupsModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        public string Name { get; set; }

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
                    command.CommandText = "select count(*) from grupo_produto";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<ProductGroupsModel> RecoverList(int page, int pageSize, string filter = "", string order = "")
        {
            var ret = new List<ProductGroupsModel>();

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

                    command.Connection = Connection;
                    command.CommandText = string.Format(
                        "select *" +
                        " from grupo_produto" +
                        filtroWhere +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                        " offset {0} rows fetch next {1} rows only",
                        pos > 0 ? pos - 1 : 0, pageSize);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new ProductGroupsModel
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

        public static ProductGroupsModel RecoverById(int id)
        {
            ProductGroupsModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from grupo_produto where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new ProductGroupsModel
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
                        command.CommandText = "delete from grupo_produto where (id = @id)";

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
                        command.CommandText = "insert into grupo_produto (nome, ativo) values (@nome, @ativo); select convert(int, scope_identity())";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);

                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText = "update grupo_produto set nome=@nome, ativo=@ativo where id = @id";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
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