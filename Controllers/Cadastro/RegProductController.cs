using ProductRegistration.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProductRegistration.Web.Controllers
{
    public class RegProductController : Controller
    {
        private const int _quantMaxLinhasPorPagina = 5;

        public ActionResult Index()
        {
            ViewBag.ListaTamPag = new SelectList(new int[] { _quantMaxLinhasPorPagina, 10, 15, 20 }, _quantMaxLinhasPorPagina);
            ViewBag.QuantMaxLinhasPorPagina = _quantMaxLinhasPorPagina;
            ViewBag.PaginaAtual = 1;

            var lista = ProductModel.RecoverList(ViewBag.PaginaAtual, _quantMaxLinhasPorPagina);
            var quant = ProductModel.RecoverQuantity();

            var difQuantPaginas = (quant % ViewBag.QuantMaxLinhasPorPagina) > 0 ? 1 : 0;
            ViewBag.QuantPaginas = (quant / ViewBag.QuantMaxLinhasPorPagina) + difQuantPaginas;
            ViewBag.UnidadesMedida = UnitsMeasureModel.RecoverList(1, 9999);
            ViewBag.Grupos = ProductGroupsModel.RecoverList(1, 9999);
            ViewBag.Marcas = BrandProductModel.RecoverList(1, 9999);
            ViewBag.Providerses = ProvidersModel.RecoverList();
            ViewBag.LocaisArmazenamento = StorageLocationsModel.RecoverList(1, 9999);

            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ProductPagina(int page, int tamPag, string order)
        {
            var lista = ProductModel.RecoverList(page, tamPag, order: order);

            return Json(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RecuperarProduct(int id)
        {
            return Json(ProductModel.RecoverById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ExcluirProduct(int id)
        {
            return Json(ProductModel.ExcludeId(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveProduct()
        {
            var resultado = "OK";
            var mensagens = new List<string>();
            var idSalvo = string.Empty;

            var nomeArquivoImagem = "";
            HttpPostedFileBase arquivo = null;
            if (Request.Files.Count > 0)
            {
                arquivo = Request.Files[0];
                nomeArquivoImagem = Guid.NewGuid().ToString() + ".jpg";
            }

            var model = new ProductModel()
            {
                Id = Int32.Parse(Request.Form["Id"]),
                Code = Request.Form["Codigo"],
                Name = Request.Form["Name"],
                PriceCost = Decimal.Parse(Request.Form["PrecoCusto"]),
                PriceSale = Decimal.Parse(Request.Form["PrecoVenda"]),
                StockQuantity = Int32.Parse(Request.Form["QuantEstoque"]),
                MeasureUnit = Int32.Parse(Request.Form["IdUnitsMeasure"]),
                IdGroup = Int32.Parse(Request.Form["IdGrupo"]),
                BrandId = Int32.Parse(Request.Form["IdMarca"]),
                SupplierId = Int32.Parse(Request.Form["IdProviders"]),
                IdLocalStorage = Int32.Parse(Request.Form["IdStorageLocations"]),
                Active = (Request.Form["Active"] == "true"),
                Image = nomeArquivoImagem
            };

            if (!ModelState.IsValid)
            {
                resultado = "AVISO";
                mensagens = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
            }
            else
            {
                try
                {
                    var nomeArquivoImagemAnterior = "";
                    if (model.Id > 0)
                    {
                        nomeArquivoImagemAnterior = ProductModel.RecoverImagePeloId(model.Id);
                    }

                    var id = model.Save();
                    if (id > 0)
                    {
                        idSalvo = id.ToString();
                        if (!string.IsNullOrEmpty(nomeArquivoImagem) && arquivo != null)
                        {
                            var diretorio = Server.MapPath("~/Content/Imagens");

                            var caminhoArquivo = Path.Combine(diretorio, nomeArquivoImagem);
                            arquivo.SaveAs(caminhoArquivo);

                            if (!string.IsNullOrEmpty(nomeArquivoImagemAnterior))
                            {
                                var caminhoArquivoAnterior = Path.Combine(diretorio, nomeArquivoImagemAnterior);
                                System.IO.File.Delete(caminhoArquivoAnterior);
                            }
                        }
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