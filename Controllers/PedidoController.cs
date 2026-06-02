using Proyec_Agricola_Web.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyec_Agricola_Web.Controllers
{
    public class PedidoController : Controller
    {
        private CarritoDAL carritoDAL = new CarritoDAL();
        private PedidoDAL pedidoDAL = new PedidoDAL();
        private ProductoDAL productoDAL = new ProductoDAL();

        [HttpGet]
        public ActionResult Checkout()
        {
            if (Session["UsuarioID"] == null)
            {
                TempData["Mensaje"] = "Debes iniciar sesión para realizar un pedido.";
                return RedirectToAction("Index", "Login");
            }

            Carrito carrito = ObtenerCarritoActual();
            if (carrito == null || carrito.DetalleItems == null || carrito.DetalleItems.Count == 0)
            {
                TempData["Mensaje"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Carrito");
            }

            decimal subtotal = carrito.DetalleItems.Sum(x => x.PrecioTotal);
            decimal ivaTasa = 0.16m;
            decimal iva = Math.Round(subtotal * ivaTasa, 2);
            decimal envio = 0;
            decimal total = subtotal + iva + envio;

            ViewBag.Subtotal = subtotal;
            ViewBag.ImpuestoTasa = ivaTasa;
            ViewBag.Impuesto = iva;
            ViewBag.CostoEnvio = envio;
            ViewBag.Total = total;

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarPedido(string direccion, string ciudad, string codigoPostal, string telefono, string notas)
        {
            if (Session["UsuarioID"] == null)
            {
                return Json(new { success = false, message = "Debes iniciar sesión." });
            }

            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(direccion))
            {
                return Json(new { success = false, message = "El campo Dirección es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(ciudad))
            {
                return Json(new { success = false, message = "El campo Ciudad es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                return Json(new { success = false, message = "El campo Código Postal es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                return Json(new { success = false, message = "El campo Teléfono de contacto es obligatorio." });
            }

            Carrito carrito = ObtenerCarritoActual();
            if (carrito == null || carrito.DetalleItems == null || carrito.DetalleItems.Count == 0)
            {
                return Json(new { success = false, message = "El carrito está vacío." });
            }

            foreach (var item in carrito.DetalleItems)
            {
                Producto producto = productoDAL.ObtenerProductoPorId(item.ProductoID);
                if (producto == null || producto.Stock < item.Cantidad)
                {
                    return Json(new { success = false, message = $"Stock insuficiente para: {item.NombreProducto}" });
                }
            }

            decimal subtotal = carrito.DetalleItems.Sum(x => x.PrecioTotal);
            decimal ivaTasa = 0.16m;
            decimal iva = Math.Round(subtotal * ivaTasa, 2);
            decimal envio = 0;
            decimal total = subtotal + iva + envio;

            var usuarioID = Convert.ToInt32(Session["UsuarioID"]);

            Pedido pedido = new Pedido
            {
                UsuarioID = usuarioID,
                DireccionEnvio = direccion,
                CiudadEnvio = ciudad,
                CodigoPostalEnvio = codigoPostal,
                TelefonoEnvio = telefono,
                PaisEnvio = "México",
                Subtotal = subtotal,
                ImpuestosTasa = ivaTasa,
                Impuestos = iva,
                CostoEnvio = envio,
                Total = total,
                Notas = notas ?? "",
                DetalleItems = carrito.DetalleItems.Select(d => new PedidoDetalle
                {
                    ProductoID = d.ProductoID,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    PrecioTotal = d.PrecioTotal
                }).ToList()
            };

            int pedidoID = pedidoDAL.CrearPedido(pedido);
            if (pedidoID > 0)
            {
                carritoDAL.VaciarCarrito(carrito.CarritoID);
                Session["CarritoCantidad"] = 0;

                return Json(new { success = true, pedidoID = pedidoID });
            }

            return Json(new { success = false, message = "Error al procesar el pedido. Intenta nuevamente." });
        }

        [HttpGet]
        public ActionResult Confirmacion(int id)
        {
            Pedido pedido = pedidoDAL.ObtenerPedidoPorId(id);
            if (pedido == null)
                return HttpNotFound();

            int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
            if (pedido.UsuarioID != usuarioID)
                return HttpNotFound();

            return View(pedido);
        }

        [HttpGet]
        public ActionResult Historial()
        {
            if (Session["UsuarioID"] == null)
            {
                TempData["Mensaje"] = "Debes iniciar sesión para ver tu historial.";
                return RedirectToAction("Index", "Login");
            }

            int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
            var pedidos = pedidoDAL.ObtenerPedidosPorUsuario(usuarioID);
            return View(pedidos);
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            Pedido pedido = pedidoDAL.ObtenerPedidoPorId(id);
            if (pedido == null)
                return HttpNotFound();

            int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
            if (pedido.UsuarioID != usuarioID)
                return HttpNotFound();

            return View(pedido);
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult ObtenerDetallePedido(int id)
        {
            try
            {
                Pedido pedido = pedidoDAL.ObtenerPedidoPorId(id);
                if (pedido == null)
                {
                    return Json(new { success = false, message = "Pedido no encontrado." }, JsonRequestBehavior.AllowGet);
                }

                // Validar que el usuario autenticado sea el dueño del pedido
                if (Session["UsuarioID"] != null)
                {
                    int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
                    if (pedido.UsuarioID != usuarioID)
                    {
                        return Json(new { success = false, message = "No tienes permiso para ver este pedido." }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        PedidoID = pedido.PedidoID,
                        NumeroPedido = pedido.NumeroPedido,
                        FechaPedido = pedido.FechaPedido,
                        FechaActualizacion = pedido.FechaActualizacion,
                        Estado = pedido.Estado,
                        DireccionEnvio = pedido.DireccionEnvio,
                        CiudadEnvio = pedido.CiudadEnvio,
                        CodigoPostalEnvio = pedido.CodigoPostalEnvio,
                        PaisEnvio = pedido.PaisEnvio,
                        TelefonoEnvio = pedido.TelefonoEnvio,
                        Subtotal = pedido.Subtotal,
                        Impuestos = pedido.Impuestos,
                        CostoEnvio = pedido.CostoEnvio,
                        Total = pedido.Total,
                        DetalleItems = pedido.DetalleItems.Select(d => new
                        {
                            PedidoDetalleID = d.PedidoDetalleID,
                            ProductoID = d.ProductoID,
                            NombreProducto = d.NombreProducto,
                            ImagenProducto = d.ImagenProducto,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            PrecioTotal = d.PrecioTotal
                        }).ToList()
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Error al obtener el pedido: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ActualizarEstadoPedido(int pedidoID, string nuevoEstado)
        {
            try
            {
                // Validar que el usuario sea administrador
                if (Session["TipoUsuario"] == null || Convert.ToInt32(Session["TipoUsuario"]) != 1)
                {
                    return Json(new { success = false, message = "No tienes permisos para realizar esta acción." });
                }

                bool resultado = pedidoDAL.ActualizarEstadoPedido(pedidoID, nuevoEstado);
                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Estado del pedido actualizado correctamente.",
                        nuevoEstado = nuevoEstado,
                        fechaActualizacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                    });
                }

                return Json(new { success = false, message = "Error al actualizar el estado del pedido." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public FileResult DescargarPDF(int id)
        {
            try
            {
                Pedido pedido = pedidoDAL.ObtenerPedidoPorId(id);
                if (pedido == null)
                    throw new Exception("Pedido no encontrado");

                // Validar que el usuario sea el dueño del pedido o administrador
                if (Session["UsuarioID"] != null)
                {
                    int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
                    int tipoUsuario = Session["TipoUsuario"] != null ? Convert.ToInt32(Session["TipoUsuario"]) : 0;

                    if (pedido.UsuarioID != usuarioID && tipoUsuario != 1)
                    {
                        throw new Exception("No tienes permiso para descargar este archivo.");
                    }
                }

                // Generar PDF (aquí puedes usar iTextSharp u otra librería)
                // Por ahora retornamos un archivo de ejemplo
                byte[] pdfBytes = GenerarPDF(pedido);

                return File(pdfBytes, "application/pdf", $"Pedido_{pedido.NumeroPedido}.pdf");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                TempData["Error"] = "Error al descargar el PDF: " + ex.Message;
                return null;
            }
        }

        private byte[] GenerarPDF(Pedido pedido)
        {
            // Placeholder para generación de PDF
            // Aquí deberías usar iTextSharp o similar
            // Por ahora retornamos un array vacío
            return System.Text.Encoding.UTF8.GetBytes("PDF Placeholder");
        }

        private Carrito ObtenerCarritoActual()
        {
            if (Session["UsuarioID"] != null)
            {
                int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
                return carritoDAL.ObtenerCarritoUsuario(usuarioID);
            }

            string sesionID = Session.SessionID;
            return carritoDAL.ObtenerCarritoPorSesion(sesionID);
        }
    }
}
