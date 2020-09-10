using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace ProductRegistration.Web.Models
{
    public class ProvidersModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fill in the name.")]
        [MaxLength(60, ErrorMessage = "The name can be a maximum of 60 characters.")]
        public string Name { get; set; }

        [MaxLength(100, ErrorMessage = "The corporate name can be a maximum of 100 characters.")]
        public string CompanyName { get; set; }

        [MaxLength(20, ErrorMessage = "The document number can be a maximum of 20 characters.")]
        public string DocumentNumber { get; set; }

        [Required]
        public TypePeople Type { get; set; }

        [Required(ErrorMessage = "Fill in the phone.")]
        [MaxLength(20, ErrorMessage = "The phone must be 20 characters long.")]
        public string telephone { get; set; }

        [Required(ErrorMessage = "Fill in the contact.")]
        [MaxLength(60, ErrorMessage = "Contact must be 60 characters.")]
        public string Contact { get; set; }

        [MaxLength(100, ErrorMessage = "The address of the address can be a maximum of 100 characters.")]
        public string PublicPlace { get; set; }

        [MaxLength(20, ErrorMessage = "The address number can be a maximum of 20 characters.")]
        public string Number { get; set; }

        [MaxLength(100, ErrorMessage = "The address complement can be a maximum of 100 characters.")]
        public string Complement { get; set; }

        [MaxLength(10, ErrorMessage = "The postal code of the address can be a maximum of 10 characters.")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Select country.")]
        public int IdCoutry { get; set; }

        [Required(ErrorMessage = "Select the state.")]
        public int IdState { get; set; }

        [Required(ErrorMessage = "Select city.")]
        public int IdCity { get; set; }

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
                    command.CommandText = "select count(*) from fornecedor";
                    ret = (int)command.ExecuteScalar();
                }
            }

            return ret;
        }

        public static List<ProvidersModel> RecoverList(int page = 0, int pageSize = 0, string filtro = "", string order = "")
        {
            var ret = new List<ProvidersModel>();

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

                    var pagecao = "";
                    if (page > 0 && pageSize > 0)
                    {
                        pagecao = string.Format(" offset {0} rows fetch next {1} rows only",
                            pos > 0 ? pos - 1 : 0, pageSize);
                    }

                    command.Connection = Connection;
                    command.CommandText =
                        "select *" +
                        " from fornecedor" +
                        filtroWhere +
                        " order by " + (!string.IsNullOrEmpty(order) ? order : "nome") +
                        pagecao;

                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ret.Add(new ProvidersModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            CompanyName = (string)reader["razao_social"],
                            DocumentNumber = (string)reader["num_documento"],
                            Type = (TypePeople)((int)reader["tipo"]),
                            telephone = (string)reader["telefone"],
                            Contact = (string)reader["contato"],
                            PublicPlace = (string)reader["logradouro"],
                            Number = (string)reader["numero"],
                            Complement = (string)reader["complemento"],
                            ZipCode = (string)reader["cep"],
                            IdCoutry = (int)reader["id_pais"],
                            IdState = (int)reader["id_estado"],
                            IdCity = (int)reader["id_cidade"],
                            Active = (bool)reader["ativo"]
                        });
                    }
                }
            }

            return ret;
        }

        public static ProvidersModel RecoverById(int id)
        {
            ProvidersModel ret = null;

            using (var Connection = new MySqlConnection())
            {
                Connection.ConnectionString = ConfigurationManager.ConnectionStrings["principal"].ConnectionString;
                Connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = Connection;
                    command.CommandText = "select * from fornecedor where (id = @id)";

                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ret = new ProvidersModel
                        {
                            Id = (int)reader["id"],
                            Name = (string)reader["nome"],
                            CompanyName = (string)reader["razao_social"],
                            DocumentNumber = (string)reader["num_documento"],
                            Type = (TypePeople)((int)reader["tipo"]),
                            telephone = (string)reader["telefone"],
                            Contact = (string)reader["contato"],
                            PublicPlace = (string)reader["logradouro"],
                            Number = (string)reader["numero"],
                            Complement = (string)reader["complemento"],
                            ZipCode = (string)reader["cep"],
                            IdCoutry = (int)reader["id_pais"],
                            IdState = (int)reader["id_estado"],
                            IdCity = (int)reader["id_cidade"],
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
                        command.CommandText = "delete from fornecedor where (id = @id)";

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
                        command.CommandText = "insert into fornecedor (nome, razao_social, num_documento, tipo, telefone, contato, logradouro," +
                            " numero, complemento, cep, id_pais, id_estado, id_cidade, ativo) values (@nome, @razao_social, @num_documento," +
                            " @tipo, @telefone, @contato, @logradouro, @numero, @complemento, @cep, @id_pais, @id_estado, @id_cidade, @ativo);" +
                            " select convert(int, scope_identity())";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@razao_social", MySqlDbType.VarChar).Value = this.CompanyName ?? "";
                        command.Parameters.Add("@num_documento", MySqlDbType.VarChar).Value = this.Number ?? "";
                        command.Parameters.Add("@tipo", MySqlDbType.Int32).Value = this.Type;
                        command.Parameters.Add("@telefone", MySqlDbType.VarChar).Value = this.telephone ?? "";
                        command.Parameters.Add("@contato", MySqlDbType.VarChar).Value = this.Contact ?? "";
                        command.Parameters.Add("@logradouro", MySqlDbType.VarChar).Value = this.PublicPlace ?? "";
                        command.Parameters.Add("@numero", MySqlDbType.VarChar).Value = this.Number ?? "";
                        command.Parameters.Add("@complemento", MySqlDbType.VarChar).Value = this.Complement ?? "";
                        command.Parameters.Add("@cep", MySqlDbType.VarChar).Value = this.ZipCode ?? "";
                        command.Parameters.Add("@id_pais", MySqlDbType.Int32).Value = this.IdCoutry;
                        command.Parameters.Add("@id_estado", MySqlDbType.Int32).Value = this.IdState;
                        command.Parameters.Add("@id_cidade", MySqlDbType.Int32).Value = this.IdCity;
                        command.Parameters.Add("@ativo", MySqlDbType.VarChar).Value = (this.Active ? 1 : 0);
                        ret = (int)command.ExecuteScalar();
                    }
                    else
                    {
                        command.CommandText = "update fornecedor set nome=@nome, razao_social=@razao_social, num_documento=@num_documento," +
                            " tipo=@tipo, telefone=@telefone, contato=@contato, logradouro=@logradouro, numero=@numero, complemento=@complemento," +
                            " cep=@cep, id_pais=@id_pais, id_estado=@id_estado, id_cidade=@id_cidade, ativo=@ativo where id = @id";

                        command.Parameters.Add("@nome", MySqlDbType.VarChar).Value = this.Name;
                        command.Parameters.Add("@razao_social", MySqlDbType.VarChar).Value = this.CompanyName ?? "";
                        command.Parameters.Add("@num_documento", MySqlDbType.VarChar).Value = this.Number ?? "";
                        command.Parameters.Add("@tipo", MySqlDbType.Int32).Value = this.Type;
                        command.Parameters.Add("@telefone", MySqlDbType.VarChar).Value = this.telephone ?? "";
                        command.Parameters.Add("@contato", MySqlDbType.VarChar).Value = this.Contact ?? "";
                        command.Parameters.Add("@logradouro", MySqlDbType.VarChar).Value = this.PublicPlace ?? "";
                        command.Parameters.Add("@numero", MySqlDbType.VarChar).Value = this.Number ?? "";
                        command.Parameters.Add("@complemento", MySqlDbType.VarChar).Value = this.Complement ?? "";
                        command.Parameters.Add("@cep", MySqlDbType.VarChar).Value = this.ZipCode ?? "";
                        command.Parameters.Add("@id_pais", MySqlDbType.Int32).Value = this.IdCoutry;
                        command.Parameters.Add("@id_estado", MySqlDbType.Int32).Value = this.IdState;
                        command.Parameters.Add("@id_cidade", MySqlDbType.Int32).Value = this.IdCity;
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