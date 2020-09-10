using ProductRegistration.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProductRegistration.Web.Controllers
{
    [Authorize(Roles = "Gerente")]
    public class RegUserController : Controller
    {
        private const int _quantMaxLinhasPorPagina = 5;
        private const string _KeyPadrao = "{$127;$188}";

        public ActionResult Index()
        {
            ViewBag.KeyPadrao = _KeyPadrao;
            ViewBag.ListaTamPag = new SelectList(new int[] { _quantMaxLinhasPorPagina, 10, 15, 20 }, _quantMaxLinhasPorPagina);
            ViewBag.QuantMaxLinhasPorPagina = _quantMaxLinhasPorPagina;
            ViewBag.PaginaAtual = 1;

            var lista = UserModel.RecoverList(ViewBag.PaginaAtual, _quantMaxLinhasPorPagina);
            var quant = ProductGroupsModel.RecoverQuantity();

            var difQuantPaginas = (quant % ViewBag.QuantMaxLinhasPorPagina) > 0 ? 1 : 0;
            ViewBag.QuantPaginas = (quant / ViewBag.QuantMaxLinhasPorPagina) + difQuantPaginas;

            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UserPagina(int page, int tamPag, string order)
        {
            var lista = UserModel.RecoverList(page, tamPag, order: order);

            return Json(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecuperarUser(int id)
        {
            return Json(UserModel.RecoverById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExcluirUser(int id)
        {
            return Json(UserModel.ExcludeId(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveUser(UserModel model)
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
                try
                {
                    if (model.Key == _KeyPadrao)
                    {
                        model.Key = "";
                    }

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