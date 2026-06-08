using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Proyec_Agricola_Web.Models
{
    public class PedidoDAL
    {
        string conexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

        public int CrearPedido(Pedido pedido)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Pedidos (UsuarioID, NumeroPedido, FechaPedido, 
                                    DireccionEnvio, CiudadEnvio, CodigoPostalEnvio, PaisEnvio, TelefonoEnvio,
                                    Subtotal, ImpuestosTasa, Impuestos, CostoEnvio, Total,
                                    Estado, Notas, FechaActualizacion)
                                    VALUES (@UsuarioID, @NumeroPedido, GETDATE(),
                                            @DireccionEnvio, @CiudadEnvio, @CodigoPostalEnvio, @PaisEnvio, @TelefonoEnvio,
                                            @Subtotal, @ImpuestosTasa, @Impuestos, @CostoEnvio, @Total,
                                            @Estado, @Notas, GETDATE());
                                    SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UsuarioID", pedido.UsuarioID);
                    cmd.Parameters.AddWithValue("@NumeroPedido", GenerarNumeroPedido());
                    cmd.Parameters.AddWithValue("@DireccionEnvio", pedido.DireccionEnvio ?? "");
                    cmd.Parameters.AddWithValue("@CiudadEnvio", pedido.CiudadEnvio ?? "");
                    cmd.Parameters.AddWithValue("@CodigoPostalEnvio", pedido.CodigoPostalEnvio ?? "");
                    cmd.Parameters.AddWithValue("@PaisEnvio", pedido.PaisEnvio ?? "México");
                    cmd.Parameters.AddWithValue("@TelefonoEnvio", pedido.TelefonoEnvio ?? "");
                    cmd.Parameters.AddWithValue("@Subtotal", pedido.Subtotal);
                    cmd.Parameters.AddWithValue("@ImpuestosTasa", pedido.ImpuestosTasa);
                    cmd.Parameters.AddWithValue("@Impuestos", pedido.Impuestos);
                    cmd.Parameters.AddWithValue("@CostoEnvio", pedido.CostoEnvio);
                    cmd.Parameters.AddWithValue("@Total", pedido.Total);
                    cmd.Parameters.AddWithValue("@Estado", "Pendiente");
                    cmd.Parameters.AddWithValue("@Notas", pedido.Notas ?? "");

                    con.Open();
                    int pedidoID = Convert.ToInt32(cmd.ExecuteScalar());

                    foreach (var item in pedido.DetalleItems)
                    {
                        string detalleQuery = @"INSERT INTO PedidoDetalle (PedidoID, ProductoID, Cantidad, PrecioUnitario, PrecioTotal)
                                                VALUES (@PedidoID, @ProductoID, @Cantidad, @PrecioUnitario, @PrecioTotal)";
                        SqlCommand cmdDetalle = new SqlCommand(detalleQuery, con);
                        cmdDetalle.Parameters.AddWithValue("@PedidoID", pedidoID);
                        cmdDetalle.Parameters.AddWithValue("@ProductoID", item.ProductoID);
                        cmdDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                        cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", item.PrecioUnitario);
                        cmdDetalle.Parameters.AddWithValue("@PrecioTotal", item.PrecioTotal);
                        cmdDetalle.ExecuteNonQuery();
                    }

                    return pedidoID;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al crear pedido: " + ex.Message);
                return 0;
            }
        }

        public Pedido ObtenerPedidoPorId(int pedidoID)
        {
            Pedido pedido = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.*, u.Nombre as NombreCliente FROM Pedidos p
                                 LEFT JOIN Usuarios u ON p.UsuarioID = u.UsuarioID
                                 WHERE p.PedidoID = @PedidoID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PedidoID", pedidoID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        pedido = MapearPedido(dr);
                    }
                }
            }

            if (pedido != null)
            {
                pedido.DetalleItems = ObtenerDetallePedido(pedidoID);
            }

            return pedido;
        }

        public List<Pedido> ObtenerPedidosPorUsuario(int usuarioID)
        {
            List<Pedido> pedidos = new List<Pedido>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.*, u.Nombre as NombreCliente FROM Pedidos p
                                 LEFT JOIN Usuarios u ON p.UsuarioID = u.UsuarioID
                                 WHERE p.UsuarioID = @UsuarioID
                                 ORDER BY p.FechaPedido DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        pedidos.Add(MapearPedido(dr));
                    }
                }
            }

            return pedidos;
        }

        public List<Pedido> ObtenerTodosPedidos()
        {
            List<Pedido> pedidos = new List<Pedido>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.*, u.Nombre as NombreCliente FROM Pedidos p
                                 LEFT JOIN Usuarios u ON p.UsuarioID = u.UsuarioID
                                 ORDER BY p.FechaPedido DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        pedidos.Add(MapearPedido(dr));
                    }
                }
            }

            return pedidos;
        }

        public bool ActualizarEstadoPedido(int pedidoID, string nuevoEstado)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "UPDATE Pedidos SET Estado = @Estado, FechaActualizacion = GETDATE() WHERE PedidoID = @PedidoID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PedidoID", pedidoID);
                    cmd.Parameters.AddWithValue("@Estado", nuevoEstado);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar estado: " + ex.Message);
                return false;
            }
        }

        public Dictionary<string, decimal> ObtenerVentasPorCategoria()
        {
            var ventas = new Dictionary<string, decimal>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT c.Nombre AS CategoriaNombre, ISNULL(SUM(pd.PrecioTotal), 0) AS TotalVentas
                                FROM PedidoDetalle pd
                                INNER JOIN Productos p ON pd.ProductoID = p.ProductoID
                                INNER JOIN Categorias c ON p.CategoriaID = c.CategoriaID
                                INNER JOIN Pedidos pe ON pd.PedidoID = pe.PedidoID
                                WHERE pe.Estado != 'Cancelado'
                                GROUP BY c.Nombre
                                ORDER BY TotalVentas DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        ventas[dr["CategoriaNombre"].ToString()] = Convert.ToDecimal(dr["TotalVentas"]);
                    }
                }
            }

            return ventas;
        }

        private string GenerarNumeroPedido()
        {
            return "AGR-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }

        private List<PedidoDetalle> ObtenerDetallePedido(int pedidoID)
        {
            List<PedidoDetalle> detalles = new List<PedidoDetalle>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT pd.PedidoDetalleID, pd.PedidoID, pd.ProductoID, pd.Cantidad, 
                                        pd.PrecioUnitario, pd.PrecioTotal,
                                        pr.Nombre as NombreProducto, pr.ImagenRuta as ImagenProducto
                                 FROM PedidoDetalle pd
                                 LEFT JOIN Productos pr ON pd.ProductoID = pr.ProductoID
                                 WHERE pd.PedidoID = @PedidoID
                                 ORDER BY pd.PedidoDetalleID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PedidoID", pedidoID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        detalles.Add(new PedidoDetalle
                        {
                            PedidoDetalleID = Convert.ToInt32(dr["PedidoDetalleID"]),
                            PedidoID = Convert.ToInt32(dr["PedidoID"]),
                            ProductoID = Convert.ToInt32(dr["ProductoID"]),
                            Cantidad = Convert.ToInt32(dr["Cantidad"]),
                            PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"]),
                            PrecioTotal = Convert.ToDecimal(dr["PrecioTotal"]),
                            NombreProducto = dr["NombreProducto"].ToString(),
                            ImagenProducto = dr["ImagenProducto"].ToString()
                        });
                    }
                }
            }

            return detalles;
        }

        private Pedido MapearPedido(SqlDataReader dr)
        {
            return new Pedido
            {
                PedidoID = Convert.ToInt32(dr["PedidoID"]),
                UsuarioID = Convert.ToInt32(dr["UsuarioID"]),
                NumeroPedido = dr["NumeroPedido"].ToString(),
                FechaPedido = Convert.ToDateTime(dr["FechaPedido"]),
                FechaEntrega = dr["FechaEntrega"] != DBNull.Value ? Convert.ToDateTime(dr["FechaEntrega"]) : (DateTime?)null,
                DireccionEnvio = dr["DireccionEnvio"].ToString(),
                CiudadEnvio = dr["CiudadEnvio"].ToString(),
                CodigoPostalEnvio = dr["CodigoPostalEnvio"].ToString(),
                PaisEnvio = dr["PaisEnvio"].ToString(),
                TelefonoEnvio = dr["TelefonoEnvio"].ToString(),
                DireccionFacturacion = dr["DireccionFacturacion"].ToString(),
                CiudadFacturacion = dr["CiudadFacturacion"].ToString(),
                CodigoPostalFacturacion = dr["CodigoPostalFacturacion"].ToString(),
                PaisFacturacion = dr["PaisFacturacion"].ToString(),
                Subtotal = Convert.ToDecimal(dr["Subtotal"]),
                ImpuestosTasa = dr["ImpuestosTasa"] != DBNull.Value ? Convert.ToDecimal(dr["ImpuestosTasa"]) : 0,
                Impuestos = dr["Impuestos"] != DBNull.Value ? Convert.ToDecimal(dr["Impuestos"]) : 0,
                CostoEnvio = dr["CostoEnvio"] != DBNull.Value ? Convert.ToDecimal(dr["CostoEnvio"]) : 0,
                Total = Convert.ToDecimal(dr["Total"]),
                Estado = dr["Estado"].ToString(),
                MetodoPago = dr["MetodoPago"].ToString(),
                EstadoPago = dr["EstadoPago"].ToString(),
                Notas = dr["Notas"].ToString(),
                Notas_Admin = dr["Notas_Admin"].ToString(),
                NumeroSeguimiento = dr["NumeroSeguimiento"].ToString(),
                Transportista = dr["Transportista"].ToString(),
                FechaActualizacion = Convert.ToDateTime(dr["FechaActualizacion"]),
                NombreCliente = dr["NombreCliente"].ToString()
            };
        }
    }
}
