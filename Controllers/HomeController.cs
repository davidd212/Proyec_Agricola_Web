using Proyec_Agricola_Web.Models;
using System;
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

            // Cargar productos relacionados inteligentemente
            // Prioriza: 1) Misma categoría, 2) Productos destacados, 3) Otros productos
            List<Producto> productosRelacionados = productDAL.ObtenerProductosRelacionados(
                id, 
                producto.CategoriaID, 
                8
            );

            ViewBag.ProductosRelacionados = productosRelacionados;
            return View("~/Views/Producto/Detalle.cshtml", producto);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnviarContacto(string nombre, string email, string telefono, string asunto, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(mensaje))
            {
                TempData["Mensaje"] = "Por favor completa todos los campos requeridos.";
                return RedirectToAction("Contact");
            }

            Mensaje nuevoMensaje = new Mensaje
            {
                Nombre = nombre,
                Email = email,
                Telefono = telefono ?? "",
                Asunto = asunto ?? "",
                MensajeTexto = mensaje,
                FechaEnvio = DateTime.Now,
                Leido = false
            };

            MensajeDAL mensajeDAL = new MensajeDAL();
            mensajeDAL.InsertarMensaje(nuevoMensaje);

            TempData["MensajeExito"] = "¡Gracias! Tu mensaje ha sido enviado exitosamente. Nos pondremos en contacto pronto.";
            return RedirectToAction("Contact");
        }
    }
}
