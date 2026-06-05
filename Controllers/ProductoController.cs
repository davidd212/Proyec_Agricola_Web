using Proyec_Agricola_Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyec_Agricola_Web.Controllers
{
    public class ProductoController : Controller
    {
        private ProductoDAL productDAL = new ProductoDAL();
        private CategoriaDAL categoriaDAL = new CategoriaDAL();

       
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // GET: Producto - Listar todos los productos
        public ActionResult Index()
        {
            List<Producto> productos = productDAL.ObtenerTodosProductos();
            return View(productos);
        }

        // GET: Producto/Detalle/5
        public ActionResult Detalle(int id)
        {
            Producto producto = productDAL.ObtenerProductoPorId(id);
            if (producto == null)
                return HttpNotFound();

            ViewBag.ProductosRelacionados = productDAL.ObtenerProductosRelacionados(
                id,
                producto.CategoriaID,
                8
            );
            return View(producto);
        }

        // GET: Producto/Categoria/1
        public ActionResult Categoria(int id)
        {
            List<Producto> productos = productDAL.ObtenerProductosPorCategoria(id);
            return View("Index", productos);
        }

        // GET: Producto/Buscar?q=semillas
        public ActionResult Buscar(string q)
        {
            if (string.IsNullOrEmpty(q))
                return View("Index", new List<Producto>());

            List<Producto> productos = productDAL.BuscarProductos(q);
            ViewBag.Busqueda = q;
            return View("Index", productos);
        }

        // GET: Producto/Destacados
        public ActionResult Destacados()
        {
            List<Producto> productos = productDAL.ObtenerProductosDestacados();
            return View("Index", productos);
        }

        // GET: Producto/Crear - SOLO ADMINISTRADOR
        [HttpGet]
        public ActionResult Crear()
        {
            if (Session["TipoUsuario"] == null || Convert.ToInt32(Session["TipoUsuario"]) != 1)
            {
                TempData["Mensaje"] = "No tienes permiso para crear productos. Solo administradores.";
                return RedirectToAction("Index");
            }

            // Pasar las categorías disponibles a la vista
            var categorias = categoriaDAL.ObtenerTodasCategorias();
            ViewBag.Categorias = categorias;

            return View();
        }

        // POST: Producto/Crear - SOLO ADMINISTRADOR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Producto modelo, HttpPostedFileBase imagenFile)
        {
            if (Session["TipoUsuario"] == null || Convert.ToInt32(Session["TipoUsuario"]) != 1)
            {
                TempData["Mensaje"] = "No tienes permiso para crear productos. Solo administradores.";
                return RedirectToAction("Index");
            }

            // Procesar imagen subida
            if (imagenFile != null && imagenFile.ContentLength > 0)
            {
                string resultado = GuardarImagen(imagenFile);
                if (resultado != null)
                {
                    modelo.Imagen = Path.GetFileName(resultado);
                    modelo.ImagenRuta = resultado;
                }
                else
                {
                    ViewBag.Mensaje = "El archivo de imagen no es válido. Use JPG, PNG, GIF o WEBP (máx. 5MB).";
                    ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                    return View(modelo);
                }
            }

            // Validar que la categoría exista
            if (!categoriaDAL.CategoriaExiste(modelo.CategoriaID))
            {
                ViewBag.Mensaje = "La categoría seleccionada no es válida.";
                ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                return View(modelo);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                string errorMsg = string.Join(", ", errors.Select(e => e.ErrorMessage));
                ViewBag.Mensaje = "Validación fallida: " + errorMsg;
                ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                return View(modelo);
            }

            if (productDAL.CrearProducto(modelo))
            {
                TempData["Mensaje"] = "✅ Producto creado exitosamente.";
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Mensaje = "Error al crear el producto. Verifica la consola de debug para más detalles.";
                ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                return View(modelo);
            }
        }

        // GET: Producto/Editar/5 - SOLO ADMINISTRADOR
        [HttpGet]
        public ActionResult Editar(int id)
        {
            if (Session["TipoUsuario"] == null || Convert.ToInt32(Session["TipoUsuario"]) != 1)
            {
                TempData["Mensaje"] = "No tienes permiso para editar productos. Solo administradores.";
                return RedirectToAction("Index");
            }

            Producto producto = productDAL.ObtenerProductoPorId(id);
            if (producto == null)
                return HttpNotFound();

            // Pasar las categorías disponibles a la vista
            var categorias = categoriaDAL.ObtenerTodasCategorias();
            ViewBag.Categorias = categorias;

            return View(producto);
        }

        // POST: Producto/Editar/5 - SOLO ADMINISTRADOR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, Producto modelo, HttpPostedFileBase imagenFile)
        {
            try
            {
                if (Session["TipoUsuario"] == null || Convert.ToInt32(Session["TipoUsuario"]) != 1)
                {
                    TempData["Mensaje"] = "No tienes permiso para editar productos. Solo administradores.";
                    return RedirectToAction("Index");
                }

                // Obtener el producto actual de la BD para mantener valores que no se envían desde el formulario
                Producto productoActual = productDAL.ObtenerProductoPorId(id);
                if (productoActual == null)
                {
                    TempData["Mensaje"] = "El producto no existe.";
                    return RedirectToAction("Index");
                }

                // Procesar nueva imagen si se subió una
                if (imagenFile != null && imagenFile.ContentLength > 0)
                {
                    try
                    {
                        string resultado = GuardarImagen(imagenFile);
                        if (resultado != null)
                        {
                            modelo.Imagen = Path.GetFileName(resultado);
                            modelo.ImagenRuta = resultado;
                        }
                        else
                        {
                            ViewBag.Mensaje = "El archivo de imagen no es válido. Use JPG, PNG, GIF o WEBP (máx. 5MB).";
                            ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                            return View(modelo);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ Error al guardar imagen: " + ex.Message);
                        System.Diagnostics.Debug.WriteLine("📍 Stack: " + ex.StackTrace);
                        ViewBag.Mensaje = "Error al procesar la imagen. Por favor, intenta nuevamente.";
                        ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                        return View(modelo);
                    }
                }
                // Si no se subió nueva imagen, mantener la existente
                else
                {
                    modelo.Imagen = productoActual.Imagen;
                    modelo.ImagenRuta = productoActual.ImagenRuta;
                }

                // Copiar propiedades que no vienen del formulario
                modelo.ProductoID = id;
                modelo.FechaCreacion = productoActual.FechaCreacion;
                modelo.FechaActualizacion = DateTime.Now;
                modelo.Visitas = productoActual.Visitas;
                modelo.Calificacion = productoActual.Calificacion;
                modelo.Activo = productoActual.Activo;
                modelo.ActivoCarrito = productoActual.ActivoCarrito;

                // Validar que la categoría exista
                if (!categoriaDAL.CategoriaExiste(modelo.CategoriaID))
                {
                    ViewBag.Mensaje = "La categoría seleccionada no es válida.";
                    ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                    return View(modelo);
                }

                // Limpiar errores de modelo relacionados con propiedades no enviadas desde el formulario
                ModelState.Remove("FechaCreacion");
                ModelState.Remove("FechaActualizacion");
                ModelState.Remove("Visitas");
                ModelState.Remove("Calificacion");
                ModelState.Remove("Activo");
                ModelState.Remove("ActivoCarrito");
                ModelState.Remove("CategoriaNombre");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    string errorMsg = string.Join("; ", errors.Select(e => e.ErrorMessage));
                    ViewBag.Mensaje = "Validación fallida: " + errorMsg;
                    ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                    return View(modelo);
                }

                if (productDAL.ActualizarProducto(modelo))
                {
                    TempData["Mensaje"] = "✅ Producto actualizado exitosamente.";
                    return RedirectToAction("Detalle", new { id });
                }
                else
                {
                    ViewBag.Mensaje = "Error al actualizar el producto. Intenta nuevamente.";
                    ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                    return View(modelo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ Error crítico en Editar: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("📍 Stack: " + ex.StackTrace);
                ViewBag.Mensaje = "Error crítico al editar el producto. Por favor, intenta nuevamente.";
                ViewBag.Categorias = categoriaDAL.ObtenerTodasCategorias();
                return View(modelo);
            }
        }

        // POST: Producto/Eliminar/5 - SOLO ADMINISTRADOR
        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            if (Session["TipoUsuario"] == null || Convert.ToInt32(Session["TipoUsuario"]) != 1)
            {
                return Json(new { success = false, message = "No tienes permiso para eliminar productos." });
            }

            if (productDAL.EliminarProducto(id))
            {
                return Json(new { success = true, message = "Producto eliminado exitosamente." });
            }
            else
            {
                return Json(new { success = false, message = "Error al eliminar el producto." });
            }
        }

        // ── Método privado: Guardar imagen en disco ──────────────────────────
        private string GuardarImagen(HttpPostedFileBase archivo)
        {
            if (archivo == null || archivo.ContentLength == 0) return null;
            if (archivo.ContentLength > 5 * 1024 * 1024) return null; // máx 5 MB

            string extension = Path.GetExtension(archivo.FileName).ToLower();
            if (!Array.Exists(ExtensionesPermitidas, e => e == extension)) return null;

            // Nombre único para evitar colisiones
            string nombreArchivo = Guid.NewGuid().ToString("N") + extension;
            string carpetaFisica = Server.MapPath("~/Content/Images/Productos/");

            if (!Directory.Exists(carpetaFisica))
                Directory.CreateDirectory(carpetaFisica);

            string rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);
            archivo.SaveAs(rutaFisica);

            // Ruta relativa para la URL
            return "/Content/Images/Productos/" + nombreArchivo;
        }
    }
}
