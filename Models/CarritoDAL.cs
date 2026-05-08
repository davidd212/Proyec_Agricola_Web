using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Proyec_Agricola_Web.Models
{
    public class CarritoDAL
    {
        string conexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

        // Obtener o crear carrito del usuario
        public Carrito ObtenerCarritoUsuario(int usuarioID)
        {
            Carrito carrito = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT CarritoID, UsuarioID, SesionID, FechaCreacion, FechaUltimaActualizacion, Activo FROM Carrito WHERE UsuarioID = @UsuarioID AND Activo = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        carrito = new Carrito
                        {
                            CarritoID = Convert.ToInt32(dr["CarritoID"]),
                            UsuarioID = Convert.ToInt32(dr["UsuarioID"]),
                            SesionID = dr["SesionID"].ToString(),
                            FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                            FechaUltimaActualizacion = Convert.ToDateTime(dr["FechaUltimaActualizacion"]),
                            Activo = Convert.ToBoolean(dr["Activo"])
                        };
                    }
                }
            }

            // Si no existe, crear uno nuevo
            if (carrito == null)
            {
                carrito = CrearCarritoUsuario(usuarioID);
            }

            // Cargar detalles del carrito
            if (carrito != null)
            {
                carrito.DetalleItems = ObtenerDetalleCarrito(carrito.CarritoID);
            }

            return carrito;
        }

        // Obtener carrito por sesion (usuarios no registrados)
        public Carrito ObtenerCarritoPorSesion(string sesionID)
        {
            Carrito carrito = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT CarritoID, UsuarioID, SesionID, FechaCreacion, FechaUltimaActualizacion, Activo FROM Carrito WHERE SesionID = @SesionID AND Activo = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SesionID", sesionID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        carrito = new Carrito
                        {
                            CarritoID = Convert.ToInt32(dr["CarritoID"]),
                            UsuarioID = dr["UsuarioID"] != DBNull.Value ? Convert.ToInt32(dr["UsuarioID"]) : (int?)null,
                            SesionID = dr["SesionID"].ToString(),
                            FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                            FechaUltimaActualizacion = Convert.ToDateTime(dr["FechaUltimaActualizacion"]),
                            Activo = Convert.ToBoolean(dr["Activo"])
                        };
                    }
                }
            }

            // Si no existe, crear uno nuevo
            if (carrito == null)
            {
                carrito = CrearCarritoPorSesion(sesionID);
            }

            // Cargar detalles del carrito
            if (carrito != null)
            {
                carrito.DetalleItems = ObtenerDetalleCarrito(carrito.CarritoID);
            }

            return carrito;
        }

        // Crear carrito para usuario registrado
        private Carrito CrearCarritoUsuario(int usuarioID)
        {
            Carrito carrito = null;

            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "INSERT INTO Carrito (UsuarioID, Activo) VALUES (@UsuarioID, 1); SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                    con.Open();
                    int carritoID = Convert.ToInt32(cmd.ExecuteScalar());

                    carrito = new Carrito
                    {
                        CarritoID = carritoID,
                        UsuarioID = usuarioID,
                        FechaCreacion = DateTime.Now,
                        FechaUltimaActualizacion = DateTime.Now,
                        Activo = true
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al crear carrito: " + ex.Message);
            }

            return carrito;
        }

        // Crear carrito por sesión
        private Carrito CrearCarritoPorSesion(string sesionID)
        {
            Carrito carrito = null;

            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "INSERT INTO Carrito (SesionID, Activo) VALUES (@SesionID, 1); SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@SesionID", sesionID);

                    con.Open();
                    int carritoID = Convert.ToInt32(cmd.ExecuteScalar());

                    carrito = new Carrito
                    {
                        CarritoID = carritoID,
                        SesionID = sesionID,
                        FechaCreacion = DateTime.Now,
                        FechaUltimaActualizacion = DateTime.Now,
                        Activo = true
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al crear carrito: " + ex.Message);
            }

            return carrito;
        }

        // Obtener detalles del carrito
        private List<CarritoDetalle> ObtenerDetalleCarrito(int carritoID)
        {
            List<CarritoDetalle> detalles = new List<CarritoDetalle>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT cd.CarritoDetalleID, cd.CarritoID, cd.ProductoID, cd.Cantidad, 
                                       cd.PrecioUnitario, cd.PrecioTotal, cd.FechaAgregado,
                                       p.Nombre as NombreProducto, p.ImagenRuta as ImagenProducto
                                FROM CarritoDetalle cd
                                LEFT JOIN Productos p ON cd.ProductoID = p.ProductoID
                                WHERE cd.CarritoID = @CarritoID
                                ORDER BY cd.FechaAgregado DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CarritoID", carritoID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        detalles.Add(new CarritoDetalle
                        {
                            CarritoDetalleID = Convert.ToInt32(dr["CarritoDetalleID"]),
                            CarritoID = Convert.ToInt32(dr["CarritoID"]),
                            ProductoID = Convert.ToInt32(dr["ProductoID"]),
                            Cantidad = Convert.ToInt32(dr["Cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"]),
                            PrecioTotal = Convert.ToDecimal(dr["PrecioTotal"]),
                            FechaAgregado = Convert.ToDateTime(dr["FechaAgregado"]),
                            NombreProducto = dr["NombreProducto"].ToString(),
                            ImagenProducto = dr["ImagenProducto"].ToString()
                        });
                    }
                }
            }

            return detalles;
        }

        // Agregar producto al carrito
        public bool AgregarProductoAlCarrito(int carritoID, int productoID, int cantidad)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    // Primero verificar si el producto ya existe en el carrito
                    string queryVerificar = "SELECT COUNT(*) FROM CarritoDetalle WHERE CarritoID = @CarritoID AND ProductoID = @ProductoID";
                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, con);
                    cmdVerificar.Parameters.AddWithValue("@CarritoID", carritoID);
                    cmdVerificar.Parameters.AddWithValue("@ProductoID", productoID);

                    con.Open();
                    int existe = (int)cmdVerificar.ExecuteScalar();
                    con.Close();

                    if (existe > 0)
                    {
                        // Actualizar cantidad
                        return ActualizarCantidadProductoCarrito(carritoID, productoID, cantidad);
                    }
                    else
                    {
                        // Obtener precio del producto
                        ProductoDAL productDAL = new ProductoDAL();
                        Producto producto = productDAL.ObtenerProductoPorId(productoID);

                        if (producto == null)
                            return false;

                        decimal precioTotal = producto.Precio * cantidad;

                        string query = @"INSERT INTO CarritoDetalle (CarritoID, ProductoID, Cantidad, PrecioUnitario, PrecioTotal)
                                        VALUES (@CarritoID, @ProductoID, @Cantidad, @PrecioUnitario, @PrecioTotal)";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@CarritoID", carritoID);
                        cmd.Parameters.AddWithValue("@ProductoID", productoID);
                        cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                        cmd.Parameters.AddWithValue("@PrecioUnitario", producto.Precio);
                        cmd.Parameters.AddWithValue("@PrecioTotal", precioTotal);

                        con.Open();
                        int resultado = cmd.ExecuteNonQuery();
                        con.Close();

                        // Actualizar fecha del carrito
                        ActualizarFechaCarrito(carritoID);

                        return resultado > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al agregar producto al carrito: " + ex.Message);
                return false;
            }
        }

        // Actualizar cantidad de producto en carrito
        public bool ActualizarCantidadProductoCarrito(int carritoID, int productoID, int cantidad)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    if (cantidad <= 0)
                    {
                        // Eliminar el producto del carrito
                        return EliminarProductoDelCarrito(carritoID, productoID);
                    }

                    string query = @"UPDATE CarritoDetalle SET Cantidad = @Cantidad,
                                    PrecioTotal = PrecioUnitario * @Cantidad
                                    WHERE CarritoID = @CarritoID AND ProductoID = @ProductoID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CarritoID", carritoID);
                    cmd.Parameters.AddWithValue("@ProductoID", productoID);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);

                    con.Open();
                    int resultado = cmd.ExecuteNonQuery();
                    con.Close();

                    // Actualizar fecha del carrito
                    ActualizarFechaCarrito(carritoID);

                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar cantidad: " + ex.Message);
                return false;
            }
        }

        // Eliminar producto del carrito
        public bool EliminarProductoDelCarrito(int carritoID, int productoID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "DELETE FROM CarritoDetalle WHERE CarritoID = @CarritoID AND ProductoID = @ProductoID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CarritoID", carritoID);
                    cmd.Parameters.AddWithValue("@ProductoID", productoID);

                    con.Open();
                    int resultado = cmd.ExecuteNonQuery();
                    con.Close();

                    // Actualizar fecha del carrito
                    ActualizarFechaCarrito(carritoID);

                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al eliminar producto: " + ex.Message);
                return false;
            }
        }

        // Vaciar carrito
        public bool VaciarCarrito(int carritoID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "DELETE FROM CarritoDetalle WHERE CarritoID = @CarritoID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CarritoID", carritoID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    // Actualizar fecha del carrito
                    ActualizarFechaCarrito(carritoID);

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al vaciar carrito: " + ex.Message);
                return false;
            }
        }

        // Actualizar fecha del carrito
        private void ActualizarFechaCarrito(int carritoID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "UPDATE Carrito SET FechaUltimaActualizacion = GETDATE() WHERE CarritoID = @CarritoID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CarritoID", carritoID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar fecha: " + ex.Message);
            }
        }
    }
}
