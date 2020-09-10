using ProductRegistration.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProductRegistration.Web.Controllers.Regastro
{
    [Authorize(Roles = "Gerente")]
    public class RegProfileController : Controller
    {
        private const int _quantMaxLinhasPorPagina = 5;

        public ActionResult Index()
        {
            ViewBag.ListaUser = UserModel.RecoverList();
            ViewBag.ListaTamPag = new SelectList(new int[] { _quantMaxLinhasPorPagina, 10, 15, 20 }, _quantMaxLinhasPorPagina);
            ViewBag.QuantMaxLinhasPorPagina = _quantMaxLinhasPorPagina;
            ViewBag.PaginaAtual = 1;

            var lista = ProfileModel.RecoverList(ViewBag.PaginaAtual, _quantMaxLinhasPorPagina);
            var quant = ProfileModel.RecoverQuantity();

            var difQuantPaginas = (quant % ViewBag.QuantMaxLinhasPorPagina) > 0 ? 1 : 0;
            ViewBag.QuantPaginas = (quant / ViewBag.QuantMaxLinhasPorPagina) + difQuantPaginas;

            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ProfilePagina(int page, int tamPag, string order)
        {
            var lista = ProfileModel.RecoverList(page, tamPag, order: order);

            return Json(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RecuperarProfile(int id)
        {
            var ret = ProfileModel.RecoverById(id);
            ret.CarregarUser();
            return Json(ret);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ExcluirProfile(int id)
        {
            return Json(ProfileModel.ExcludeId(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveProfile(ProfileModel model, List<int> idUser)
        {
            var resultado = "OK";
            var mensagens = new List<string>();
            var idSalvo = string.Empty;

            if (!ModelState.IsValid)
            {
                resultado = "AVISO";
                mensagens = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
            }
            else
            {
                model.User = new List<UserModel>();
                if (idUser == null || idUser.Count == 0)
                {
                    model.User.Add(new UserModel() { Id = -1 });
                }
                else
                {
                    foreach (var id in idUser)
                    {
                        model.User.Add(new UserModel() { Id = id });
                    }
                }

                try
                {
                    var id = model.Save();
                    if (id > 0)
                    {
                        idSalvo = id.ToString();
                    }
                    else
                    {
                        resultado = "ERRO";
                    }
                }
                catch (Exception)
                {
                    resultado = "ERRO";
                }
            }

            return Json(new { Resultado = resultado, Mensagens = mensagens, IdSalvo = idSalvo });
        }
    }
}