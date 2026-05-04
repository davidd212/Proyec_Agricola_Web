using Proyec_Agricola_Web.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Proyec_Agricola_Web.Controllers
{
    public class CarritoController : Controller
    {
        private CarritoDAL carritoDAL = new CarritoDAL();
        private ProductoDAL productoDAL = new ProductoDAL();

        // GET: Carrito
        public ActionResult Index()
        {
            Carrito carrito = ObtenerCarritoActual();
            return View(carrito);
        }

        // POST: Carrito/Agregar
        [HttpPost]
        public ActionResult Agregar(int productoID, int cantidad = 1)
        {
            try
            {
                // Validar que el usuario sea CLIENTE (TipoUsuario = 2) o no esté logueado
                if (Session["TipoUsuario"] != null && Convert.ToInt32(Session["TipoUsuario"]) == 1)
                {
                    return Json(new { success = false, message = "Los administradores no pueden usar el carrito." });
                }

                // Validar cantidad
                if (cantidad <= 0)
                {
                    return Json(new { success = false, message = "La cantidad debe ser mayor a 0." });
                }

                // Obtener producto
                Producto producto = productoDAL.ObtenerProductoPorId(productoID);
                if (producto == null || !producto.ActivoCarrito || producto.Stock <= 0)
                {
                    return Json(new { success = false, message = "El producto no está disponible." });
                }

                // Validar stock
                if (cantidad > producto.Stock)
                {
                    return Json(new { success = false, message = "Stock insuficiente. Disponible: " + producto.Stock });
                }

                // Obtener carrito
                Carrito carrito = ObtenerCarritoActual();
                if (carrito == null)
                {
                    return Json(new { success = false, message = "Error al acceder al carrito." });
                }

                // Agregar producto
                if (carritoDAL.AgregarProductoAlCarrito(carrito.CarritoID, productoID, cantidad))
                {
                    // Actualizar cantidad de items en el carrito para mostrar en header
                    carrito = ObtenerCarritoActual();
                    Session["CarritoCantidad"] = carrito.DetalleItems.Count;

                    return Json(new { success = true, message = "Producto agregado al carrito.", cantidad = carrito.DetalleItems.Count });
                }
                else
                {
                    return Json(new { success = false, message = "Error al agregar el producto." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en Agregar: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Carrito/ActualizarCantidad
        [HttpPost]
        public ActionResult ActualizarCantidad(int productoID, int cantidad)
        {
            try
            {
                // Validar que no sea administrador
                if (Session["TipoUsuario"] != null && Convert.ToInt32(Session["TipoUsuario"]) == 1)
                {
                    return Json(new { success = false, message = "Los administradores no pueden usar el carrito." });
                }

                Carrito carrito = ObtenerCarritoActual();
                if (carrito == null)
                {
                    return Json(new { success = false, message = "Error al acceder al carrito." });
                }

                if (cantidad <= 0)
                {
                    // Eliminar producto
                    if (carritoDAL.EliminarProductoDelCarrito(carrito.CarritoID, productoID))
                    {
                        carrito = ObtenerCarritoActual();
                        Session["CarritoCantidad"] = carrito.DetalleItems.Count;
                        CalcularTotalCarrito(carrito);
                        return Json(new { success = true, message = "Producto eliminado del carrito.", cantidad = carrito.DetalleItems.Count });
                    }
                }
                else
                {
                    if (carritoDAL.ActualizarCantidadProductoCarrito(carrito.CarritoID, productoID, cantidad))
                    {
                        carrito = ObtenerCarritoActual();
                        CalcularTotalCarrito(carrito);
                        return Json(new { success = true, message = "Carrito actualizado.", cantidad = carrito.DetalleItems.Count });
                    }
                }

                return Json(new { success = false, message = "Error al actualizar el carrito." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ActualizarCantidad: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Carrito/Eliminar
        [HttpPost]
        public ActionResult Eliminar(int productoID)
        {
            try
            {
                // Validar que no sea administrador
                if (Session["TipoUsuario"] != null && Convert.ToInt32(Session["TipoUsuario"]) == 1)
                {
                    return Json(new { success = false, message = "Los administradores no pueden usar el carrito." });
                }

                Carrito carrito = ObtenerCarritoActual();
                if (carrito == null)
                {
                    return Json(new { success = false, message = "Error al acceder al carrito." });
                }

                if (carritoDAL.EliminarProductoDelCarrito(carrito.CarritoID, productoID))
                {
                    carrito = ObtenerCarritoActual();
                    Session["CarritoCantidad"] = carrito.DetalleItems.Count;
                    return Json(new { success = true, message = "Producto eliminado.", cantidad = carrito.DetalleItems.Count });
                }

                return Json(new { success = false, message = "Error al eliminar el producto." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en Eliminar: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Carrito/Vaciar
        [HttpPost]
        public ActionResult Vaciar()
        {
            try
            {
                // Validar que no sea administrador
                if (Session["TipoUsuario"] != null && Convert.ToInt32(Session["TipoUsuario"]) == 1)
                {
                    return Json(new { success = false, message = "Los administradores no pueden usar el carrito." });
                }

                Carrito carrito = ObtenerCarritoActual();
                if (carrito == null)
                {
                    return Json(new { success = false, message = "Error al acceder al carrito." });
                }

                if (carritoDAL.VaciarCarrito(carrito.CarritoID))
                {
                    Session["CarritoCantidad"] = 0;
                    return Json(new { success = true, message = "Carrito vaciado." });
                }

                return Json(new { success = false, message = "Error al vaciar el carrito." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en Vaciar: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Carrito/ObtenerCarritoJSON (para AJAX)
        [HttpGet]
        public ActionResult ObtenerCarritoJSON()
        {
            try
            {
                Carrito carrito = ObtenerCarritoActual();
                decimal total = CalcularTotalCarrito(carrito);

                return Json(new
                {
                    success = true,
                    carritoID = carrito.CarritoID,
                    cantidad = carrito.DetalleItems.Count,
                    total = total,
                    items = carrito.DetalleItems
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerCarritoJSON: " + ex.Message);
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // MÉTODOS PRIVADOS
        private Carrito ObtenerCarritoActual()
        {
            Carrito carrito = null;

            // Si el usuario está logueado
            if (Session["UsuarioID"] != null)
            {
                int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
                carrito = carritoDAL.ObtenerCarritoUsuario(usuarioID);
            }
            else
            {
                // Para usuarios no registrados, usar sesión
                string sesionID = Session.SessionID;
                carrito = carritoDAL.ObtenerCarritoPorSesion(sesionID);
            }

            return carrito;
        }

        private decimal CalcularTotalCarrito(Carrito carrito)
        {
            decimal total = 0;
            if (carrito != null && carrito.DetalleItems != null)
            {
                foreach (var item in carrito.DetalleItems)
                {
                    total += item.PrecioTotal;
                }
            }
            return total;
        }
    }
}
