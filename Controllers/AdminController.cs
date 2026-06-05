using Proyec_Agricola_Web.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyec_Agricola_Web.Controllers
{
    public class AdminController : Controller
    {
        private ProductoDAL productDAL = new ProductoDAL();
        private PedidoDAL pedidoDAL = new PedidoDAL();
        private MensajeDAL mensajeDAL = new MensajeDAL();
        private UsuarioDAL usuarioDAL = new UsuarioDAL();

        private bool EsAdmin()
        {
            return Session["TipoUsuario"] != null && Convert.ToInt32(Session["TipoUsuario"]) == 1;
        }

        private ActionResult RedirectSiNoAdmin()
        {
            if (!EsAdmin())
            {
                TempData["Mensaje"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        public ActionResult Dashboard()
        {
            var redirect = RedirectSiNoAdmin();
            if (redirect != null) return redirect;

            var todosProductos = productDAL.ObtenerTodosProductos();
            var todosPedidos = pedidoDAL.ObtenerTodosPedidos();
            var usuarios = usuarioDAL.ObtenerTodosUsuarios();
            int mensajesNoLeidos = mensajeDAL.ObtenerMensajesNoLeidosCount();

            ViewBag.TotalProductos = todosProductos.Count;
            ViewBag.TotalPedidos = todosPedidos.Count;
            ViewBag.TotalUsuarios = usuarios.Count(u => u.TipoUsuario == 2);
            ViewBag.MensajesNoLeidos = mensajesNoLeidos;

            ViewBag.PedidosPendientes = todosPedidos.Count(p => p.Estado == "Pendiente");
            ViewBag.PedidosProcesando = todosPedidos.Count(p => p.Estado == "Procesando");
            ViewBag.PedidosEnviados = todosPedidos.Count(p => p.Estado == "Enviado");
            ViewBag.PedidosEntregados = todosPedidos.Count(p => p.Estado == "Entregado");
            ViewBag.PedidosCancelados = todosPedidos.Count(p => p.Estado == "Cancelado");

            ViewBag.ProductosStockBajo = todosProductos.Where(p => p.Stock <= 10).OrderBy(p => p.Stock).Take(10).ToList();
            ViewBag.UltimosPedidos = todosPedidos.Take(5).ToList();
            ViewBag.IngresosTotales = todosPedidos.Where(p => p.Estado != "Cancelado").Sum(p => p.Total);

            return View();
        }

        public ActionResult Pedidos(string estado, DateTime? desde, DateTime? hasta)
        {
            var redirect = RedirectSiNoAdmin();
            if (redirect != null) return redirect;

            var pedidos = pedidoDAL.ObtenerTodosPedidos();

            if (!string.IsNullOrEmpty(estado))
                pedidos = pedidos.Where(p => p.Estado == estado).ToList();

            if (desde.HasValue)
                pedidos = pedidos.Where(p => p.FechaPedido >= desde.Value).ToList();

            if (hasta.HasValue)
                pedidos = pedidos.Where(p => p.FechaPedido <= hasta.Value.AddDays(1)).ToList();

            ViewBag.EstadoSeleccionado = estado;
            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;

            return View(pedidos);
        }

        public ActionResult DetallePedido(int id)
        {
            var redirect = RedirectSiNoAdmin();
            if (redirect != null) return redirect;

            Pedido pedido = pedidoDAL.ObtenerPedidoPorId(id);
            if (pedido == null)
                return HttpNotFound();

            return View(pedido);
        }

        [HttpPost]
        public JsonResult CambiarEstadoPedido(int pedidoID, string nuevoEstado)
        {
            if (!EsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos." });
            }

            bool resultado = pedidoDAL.ActualizarEstadoPedido(pedidoID, nuevoEstado);
            if (resultado)
            {
                return Json(new
                {
                    success = true,
                    message = "Estado actualizado correctamente.",
                    nuevoEstado = nuevoEstado,
                    fechaActualizacion = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                });
            }

            return Json(new { success = false, message = "Error al actualizar el estado." });
        }

        [HttpPost]
        public JsonResult GuardarNotasAdmin(int pedidoID, string notas)
        {
            if (!EsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos." });
            }

            // Use a direct update for notas_admin
            using (var con = new System.Data.SqlClient.SqlConnection(
                System.Configuration.ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString))
            {
                string query = "UPDATE Pedidos SET Notas_Admin = @Notas, FechaActualizacion = GETDATE() WHERE PedidoID = @PedidoID";
                var cmd = new System.Data.SqlClient.SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PedidoID", pedidoID);
                cmd.Parameters.AddWithValue("@Notas", notas ?? "");

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true, message = "Notas guardadas." });
        }

        public ActionResult Mensajes()
        {
            var redirect = RedirectSiNoAdmin();
            if (redirect != null) return redirect;

            var mensajes = mensajeDAL.ObtenerTodosMensajes();
            ViewBag.NoLeidos = mensajes.Count(m => !m.Leido);
            return View(mensajes);
        }

        public ActionResult VerMensaje(int id)
        {
            var redirect = RedirectSiNoAdmin();
            if (redirect != null) return redirect;

            Mensaje mensaje = mensajeDAL.ObtenerMensajePorId(id);
            if (mensaje == null)
                return HttpNotFound();

            if (!mensaje.Leido)
            {
                mensajeDAL.MarcarComoLeido(id);
                mensaje.Leido = true;
                mensaje.FechaLeido = DateTime.Now;
            }

            return View(mensaje);
        }

        [HttpPost]
        public JsonResult EliminarMensaje(int id)
        {
            if (!EsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos." });
            }

            bool resultado = mensajeDAL.EliminarMensaje(id);
            return Json(new { success = resultado, message = resultado ? "Mensaje eliminado." : "Error al eliminar." });
        }
    }
}