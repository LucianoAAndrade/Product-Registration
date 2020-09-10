using ProductRegistration.Web.Models;
using System.Security.Principal;

namespace ProductRegistration.Web
{
    public class AplicacaoPrincipal : GenericPrincipal
    {
        public UserModel Dados { get; set; }

        public AplicacaoPrincipal(IIdentity identity, string[] roles, int id) : base(identity, roles)
        {
            Dados = UserModel.RecoverById(id);
        }
    }
}