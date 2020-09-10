using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class ProductModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the code.")]
        [MaxLength(10, ErrorMessage = "The code can be a maximum of 10 characters.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        [MaxLength(50, ErrorMessage = "The name can be a maximum of 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Fill in the cost price.")]
        public decimal PriceCost { get; set; }

        [Required(ErrorMessage = "Fill in the sales price.")]
        public decimal PriceSale { get; set; }

        [Required(ErrorMessage = "Fill in stock quantity.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Select the unit of measurement.")]
        public int MeasureUnit { get; set; }

        [Required(ErrorMessage = "Select group.")]
        public int IdGroup { get; set; }

        [Required(ErrorMessage = "Select brand.")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Select supplier.")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Select the storage location.")]
        public int IdLocalStorage { get; set; }

        public bool Active { get; set; }

        public string Image { get; set; }

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
                    command.CommandText = "select count(*) from produto";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        private static ProductModel MountProduct(MySqlDataReader reader)
        {
            return new ProductModel
            {
                Id = (int)reader["id"],
                Code = (string)reader["codigo"],
                Name = (string)reader["nome"],
                PriceCost = (decimal)reader["preco_custo"],
                PriceSale = (decimal)reader["preco_venda"],
                StockQuantity = (int)reader["quant_estoque"],
                MeasureUnit = (int)reader["id_unidade_medida"],
                IdGroup = (int)reader["id_grupo"],
                BrandId = (int)reader["id_marca"],
                SupplierId = (int)reader["id_fornecedor"],
                IdLocalStorage = (int)reader["id_local_armazenamento"],
                Active = (bool)reader["ativo"],
                Image = (string)reader["imagem"],
            };
        }

        public static List<ProductModel> RecoverList(int page = 0, int pageSize = 0, string filter = "", string order = "", bool somenteActives = false)
        {
            var ret = new List<ProductModel>();

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
                        filtroWhere = string.Format(" where (lower(nome) like '%{0}%')", filter.ToLower());
                    }

                    if (somenteActives)
                    {
                        filtroWhere = (string.IsNullOrEmpty(filtroWhere) ? " where" : " and") + "(ativo = 1)";
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
                        " from produto" +
                        filtroWhere +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                        pagecao;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(MountProduct(reader));
                    }
                }
            }

            return ret;
        }

        public static ProductModel RecoverById(int id)
        {
            ProductModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from produto where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = MountProduct(reader);
                    }
                }
            }

            return ret;
        }

        public static string RecoverImagePeloId(int id)
        {
            string ret = "";

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select imagem from produto where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = (string)reader["imagem"];
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
                        command.CommandText = "delete from produto where (id = @id)";

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
                        command.CommandText =
                            "insert into produto " +
                            "(codigo, nome, preco_custo, preco_venda, quant_estoque, id_unidade_medida, id_grupo, id_marca, " +
                            "id_fornecedor, id_local_armazenamento, ativo, imagem) values " +
                            "(@codigo, @nome, @preco_custo, @preco_venda, @quant_estoque, @id_unidade_medida, @id_grupo, @id_marca, " +
                            "@id_fornecedor, @id_local_armazenamento, @ativo, @imagem); select convert(int, scope_identity())";

                        command.Parameters.Add("@codigo", MySqlDbType.VarChar).Value = this.Code;
                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@preco_custo", MySqlDbType.Decimal).Value = this.PriceCost;
                        command.Parameters.Add("@preco_venda", MySqlDbType.Decimal).Value = this.PriceSale;
                        command.Parameters.Add("@quant_estoque", MySqlDbType.Int32).Value = this.StockQuantity;
                        command.Parameters.Add("@id_unidade_medida", MySqlDbType.Int32).Value = this.MeasureUnit;
                        command.Parameters.Add("@id_grupo", MySqlDbType.Int32).Value = this.IdGroup;
                        command.Parameters.Add("@id_marca", MySqlDbType.Int32).Value = this.BrandId;
                        command.Parameters.Add("@id_fornecedor", MySqlDbType.Int32).Value = this.SupplierId;
                        command.Parameters.Add("@id_local_armazenamento", MySqlDbType.Int32).Value = this.IdLocalStorage;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);
                        command.Parameters.Add("@imagem", MySqlDbType.VarChar).Value = this.Image;

                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText =
                            "update produto set codigo=@codigo, nome=@nome, preco_custo=@preco_custo, " +
                            "preco_venda=@preco_venda, quant_estoque=@quant_estoque, id_unidade_medida=@id_unidade_medida, " +
                            "id_grupo=@id_grupo, id_marca=@id_marca, id_fornecedor=@id_fornecedor, " +
                            "id_local_armazenamento=@id_local_armazenamento, ativo=@ativo, imagem=@imagem where id = @id";

                        command.Parameters.Add("@id", MySqlDbType.Int32).Value = this.Id;
                        command.Parameters.Add("@codigo", MySqlDbType.VarChar).Value = this.Code;
                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@preco_custo", MySqlDbType.Decimal).Value = this.PriceCost;
                        command.Parameters.Add("@preco_venda", MySqlDbType.Decimal).Value = this.PriceSale;
                        command.Parameters.Add("@quant_estoque", MySqlDbType.Int32).Value = this.StockQuantity;
                        command.Parameters.Add("@id_unidade_medida", MySqlDbType.Int32).Value = this.MeasureUnit;
                        command.Parameters.Add("@id_grupo", MySqlDbType.Int32).Value = this.IdGroup;
                        command.Parameters.Add("@id_marca", MySqlDbType.Int32).Value = this.BrandId;
                        command.Parameters.Add("@id_fornecedor", MySqlDbType.Int32).Value = this.SupplierId;
                        command.Parameters.Add("@id_local_armazenamento", MySqlDbType.Int32).Value = this.IdLocalStorage;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);
                        command.Parameters.Add("@imagem", MySqlDbType.VarChar).Value = this.Image;


                        if (command.ExecuteNonQuery() > 0)
                        {
                            ret = this.Id;
                        }
                    }
                }
            }

            return ret;
        }

        public static string SaveRequestEntry(DateTime date, Dictionary<int, int> product)
        {
            return SaveRequest(date, product, "entrada_produto", true);
        }

        public static string SaveRequestOutbound(DateTime date, Dictionary<int, int> product)
        {
            return SaveRequest(date, product, "saida_produto", false);
        }

        public static string SaveRequest (DateTime date, Dictionary<int, int> product, string nameTable, bool input)
        {
            var ret = "";

            try
            {
                using (var Connection = new MySqlConnection())
                {
                    Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                    Connection.Open();

                    var numPedido = "";
                    using (var command = new MySqlCommand())
                    {
                        command.Connection = Connection;
                        command.CommandText = $"select next value for sec_{nameTable}";
                        numPedido = ((int)command.ExecuteScalar()).ToString("D10");
                    }

                    using (var transaction = Connection.BeginTransaction())
                    {
                        foreach (var produto in product)
                        {
                            using (var command = new MySqlCommand())
                            {
                                command.Connection = Connection;
                                command.Transaction = transaction;
                                command.CommandText = $"insert into {nameTable} (numero, data, id_produto, quant) values (@numero, @data, @id_produto, @quant)";

                                command.Parameters.Add("@numero", MySqlDbType.VarChar).Value = numPedido;
                                command.Parameters.Add("@data", MySqlDbType.Date).Value = date;
                                command.Parameters.Add("@id_produto", MySqlDbType.Int32).Value = produto.Key;
                                command.Parameters.Add("@quant", MySqlDbType.Int32).Value = produto.Value;

                                command.ExecuteNonQuery();
                            }

                            using (var command = new MySqlCommand())
                            {
                                var sinal = (input ? "+" : "-");
                                command.Connection = Connection;
                                command.Transaction = transaction;
                                command.CommandText = $"update produto set quant_estoque = quant_estoque {sinal} @quant_estoque where (id = @id)";

                                command.Parameters.Add("@id", MySqlDbType.Int32).Value = produto.Key;
                                command.Parameters.Add("@quant_estoque", MySqlDbType.Int32).Value = produto.Value;

                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        ret = numPedido;
                    }
                }
            }
            catch (Exception)
            {
            }

            return ret;
        }
    }
}