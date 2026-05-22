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

            if (string.IsNullOrEmpty(direccion) || string.IsNullOrEmpty(ciudad))
            {
                return Json(new { success = false, message = "Completa los campos de dirección y ciudad." });
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
                CodigoPostalEnvio = codigoPostal ?? "",
                TelefonoEnvio = telefono ?? "",
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
