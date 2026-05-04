using Proyec_Agricola_Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Proyec_Agricola_Web.Controllers
{
    public class HomeController : Controller
    {
        private ProductoDAL productDAL = new ProductoDAL();

        public ActionResult Index()
        {
            // Cargar productos destacados desde la base de datos
            List<Producto> destacados = productDAL.ObtenerProductosDestacados();
            return View(destacados);
        }

        public ActionResult Productos()
        {
            List<Producto> productos = productDAL.ObtenerTodosProductos();
            return View(productos);
        }

        public ActionResult Categorias()
        {
            List<Producto> productos = productDAL.ObtenerTodosProductos();
            var categorias = productos.Select(p => p.CategoriaNombre).Distinct().ToList();
            return View(categorias);
        }

        public ActionResult ProductosPorCategoria(string categoria)
        {
            List<Producto> productos = productDAL.ObtenerTodosProductos();
            var productosFiltrados = productos.Where(p => p.CategoriaNombre == categoria).ToList();
            ViewBag.Categoria = categoria;
            return View("Productos", productosFiltrados);
        }

        public ActionResult BuscarProductos(string q)
        {
            if (string.IsNullOrEmpty(q))
                return View("Productos", new List<Producto>());

            List<Producto> resultados = productDAL.BuscarProductos(q);
            ViewBag.Busqueda = q;
            return View("Productos", resultados);
        }

        public ActionResult DetalleProducto(int id)
        {
            Producto producto = productDAL.ObtenerProductoPorId(id);
            if (producto == null)
                return HttpNotFound();
            return View(producto);
        }

        public ActionResult Carrito()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Sobre AgroVentas";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contacto - AgroVentas";
            return View();
        }
    }
}
