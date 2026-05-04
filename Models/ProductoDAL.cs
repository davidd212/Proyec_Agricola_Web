using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Proyec_Agricola_Web.Models
{
    public class ProductoDAL
    {
        string conexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

        // Obtener todos los productos activos
        public List<Producto> ObtenerTodosProductos()
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.ProductoID, p.CategoriaID, p.Nombre, p.Descripcion, p.DescripcionCorta, 
                                       p.Precio, p.PrecioDescuento, p.Stock, p.Imagen, p.ImagenRuta, p.CodigoProducto,
                                       p.Peso, p.Activo, p.ActivoCarrito, p.Destacado, p.Visitas, p.Calificacion,
                                       p.FechaCreacion, p.FechaActualizacion, c.Nombre as CategoriaNombre
                                FROM Productos p
                                LEFT JOIN Categorias c ON p.CategoriaID = c.CategoriaID
                                WHERE p.Activo = 1 AND c.Activa = 1
                                ORDER BY p.FechaCreacion DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        productos.Add(MapearProducto(dr));
                    }
                }
            }

            return productos;
        }

        // Obtener producto por ID
        public Producto ObtenerProductoPorId(int productoID)
        {
            Producto producto = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.ProductoID, p.CategoriaID, p.Nombre, p.Descripcion, p.DescripcionCorta,
                                       p.Precio, p.PrecioDescuento, p.Stock, p.Imagen, p.ImagenRuta, p.CodigoProducto,
                                       p.Peso, p.Activo, p.ActivoCarrito, p.Destacado, p.Visitas, p.Calificacion,
                                       p.FechaCreacion, p.FechaActualizacion, c.Nombre as CategoriaNombre
                                FROM Productos p
                                LEFT JOIN Categorias c ON p.CategoriaID = c.CategoriaID
                                WHERE p.ProductoID = @ProductoID AND p.Activo = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductoID", productoID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        producto = MapearProducto(dr);
                    }
                }
            }

            return producto;
        }

        // Obtener productos por categoría
        public List<Producto> ObtenerProductosPorCategoria(int categoriaID)
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.ProductoID, p.CategoriaID, p.Nombre, p.Descripcion, p.DescripcionCorta,
                                       p.Precio, p.PrecioDescuento, p.Stock, p.Imagen, p.ImagenRuta, p.CodigoProducto,
                                       p.Peso, p.Activo, p.ActivoCarrito, p.Destacado, p.Visitas, p.Calificacion,
                                       p.FechaCreacion, p.FechaActualizacion, c.Nombre as CategoriaNombre
                                FROM Productos p
                                LEFT JOIN Categorias c ON p.CategoriaID = c.CategoriaID
                                WHERE p.CategoriaID = @CategoriaID AND p.Activo = 1 AND c.Activa = 1
                                ORDER BY p.Nombre";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CategoriaID", categoriaID);

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        productos.Add(MapearProducto(dr));
                    }
                }
            }

            return productos;
        }

        // Buscar productos
        public List<Producto> BuscarProductos(string termino)
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT p.ProductoID, p.CategoriaID, p.Nombre, p.Descripcion, p.DescripcionCorta,
                                       p.Precio, p.PrecioDescuento, p.Stock, p.Imagen, p.ImagenRuta, p.CodigoProducto,
                                       p.Peso, p.Activo, p.ActivoCarrito, p.Destacado, p.Visitas, p.Calificacion,
                                       p.FechaCreacion, p.FechaActualizacion, c.Nombre as CategoriaNombre
                                FROM Productos p
                                LEFT JOIN Categorias c ON p.CategoriaID = c.CategoriaID
                                WHERE p.Activo = 1 AND c.Activa = 1 AND 
                                      (p.Nombre LIKE @Termino OR p.Descripcion LIKE @Termino OR p.DescripcionCorta LIKE @Termino)
                                ORDER BY p.Nombre";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Termino", "%" + termino + "%");

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        productos.Add(MapearProducto(dr));
                    }
                }
            }

            return productos;
        }

        // Obtener productos destacados
        public List<Producto> ObtenerProductosDestacados(int cantidad = 6)
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = @"SELECT TOP " + cantidad + @" p.ProductoID, p.CategoriaID, p.Nombre, p.Descripcion, p.DescripcionCorta,
                                       p.Precio, p.PrecioDescuento, p.Stock, p.Imagen, p.ImagenRuta, p.CodigoProducto,
                                       p.Peso, p.Activo, p.ActivoCarrito, p.Destacado, p.Visitas, p.Calificacion,
                                       p.FechaCreacion, p.FechaActualizacion, c.Nombre as CategoriaNombre
                                FROM Productos p
                                LEFT JOIN Categorias c ON p.CategoriaID = c.CategoriaID
                                WHERE p.Activo = 1 AND p.Destacado = 1 AND c.Activa = 1
                                ORDER BY p.FechaCreacion DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        productos.Add(MapearProducto(dr));
                    }
                }
            }

            return productos;
        }

        // Crear nuevo producto (SOLO ADMINISTRADOR)
        public bool CrearProducto(Producto producto)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Productos (CategoriaID, Nombre, Descripcion, DescripcionCorta, 
                                    Precio, PrecioDescuento, Stock, Imagen, ImagenRuta, CodigoProducto, Peso, 
                                    Activo, ActivoCarrito, Destacado, FechaCreacion, FechaActualizacion, Visitas, Calificacion)
                                    VALUES (@CategoriaID, @Nombre, @Descripcion, @DescripcionCorta, 
                                            @Precio, @PrecioDescuento, @Stock, @Imagen, @ImagenRuta, @CodigoProducto, 
                                            @Peso, @Activo, @ActivoCarrito, @Destacado, GETDATE(), GETDATE(), 0, 0)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CategoriaID", producto.CategoriaID);
                    cmd.Parameters.AddWithValue("@Nombre", producto.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? "");
                    cmd.Parameters.AddWithValue("@DescripcionCorta", producto.DescripcionCorta ?? "");
                    cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                    cmd.Parameters.AddWithValue("@PrecioDescuento", producto.PrecioDescuento.HasValue ? (object)producto.PrecioDescuento : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Stock", producto.Stock);
                    cmd.Parameters.AddWithValue("@Imagen", producto.Imagen ?? "");
                    cmd.Parameters.AddWithValue("@ImagenRuta", producto.ImagenRuta ?? "");
                    cmd.Parameters.AddWithValue("@CodigoProducto", producto.CodigoProducto ?? "");
                    cmd.Parameters.AddWithValue("@Peso", producto.Peso.HasValue ? (object)producto.Peso : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", 1);
                    cmd.Parameters.AddWithValue("@ActivoCarrito", 1);
                    cmd.Parameters.AddWithValue("@Destacado", producto.Destacado ? 1 : 0);

                    con.Open();
                    int resultado = cmd.ExecuteNonQuery();
                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("? Error al crear producto: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("?? Stack Trace: " + ex.StackTrace);
                return false;
            }
        }

        // Actualizar producto (SOLO ADMINISTRADOR)
        public bool ActualizarProducto(Producto producto)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"UPDATE Productos SET CategoriaID = @CategoriaID, Nombre = @Nombre, 
                                    Descripcion = @Descripcion, DescripcionCorta = @DescripcionCorta,
                                    Precio = @Precio, PrecioDescuento = @PrecioDescuento, Stock = @Stock, 
                                    Imagen = @Imagen, ImagenRuta = @ImagenRuta, CodigoProducto = @CodigoProducto,
                                    Peso = @Peso, Destacado = @Destacado, FechaActualizacion = GETDATE()
                                    WHERE ProductoID = @ProductoID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ProductoID", producto.ProductoID);
                    cmd.Parameters.AddWithValue("@CategoriaID", producto.CategoriaID);
                    cmd.Parameters.AddWithValue("@Nombre", producto.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion ?? "");
                    cmd.Parameters.AddWithValue("@DescripcionCorta", producto.DescripcionCorta ?? "");
                    cmd.Parameters.AddWithValue("@Precio", producto.Precio);
                    cmd.Parameters.AddWithValue("@PrecioDescuento", producto.PrecioDescuento.HasValue ? (object)producto.PrecioDescuento : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Stock", producto.Stock);
                    cmd.Parameters.AddWithValue("@Imagen", producto.Imagen ?? "");
                    cmd.Parameters.AddWithValue("@ImagenRuta", producto.ImagenRuta ?? "");
                    cmd.Parameters.AddWithValue("@CodigoProducto", producto.CodigoProducto ?? "");
                    cmd.Parameters.AddWithValue("@Peso", producto.Peso.HasValue ? (object)producto.Peso : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Destacado", producto.Destacado ? 1 : 0);

                    con.Open();
                    int resultado = cmd.ExecuteNonQuery();
                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar producto: " + ex.Message);
                return false;
            }
        }

        // Eliminar producto (SOLO ADMINISTRADOR)
        public bool EliminarProducto(int productoID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "UPDATE Productos SET Activo = 0 WHERE ProductoID = @ProductoID";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ProductoID", productoID);

                    con.Open();
                    int resultado = cmd.ExecuteNonQuery();
                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al eliminar producto: " + ex.Message);
                return false;
            }
        }

        private Producto MapearProducto(SqlDataReader dr)
        {
            return new Producto
            {
                ProductoID = Convert.ToInt32(dr["ProductoID"]),
                CategoriaID = Convert.ToInt32(dr["CategoriaID"]),
                Nombre = dr["Nombre"].ToString(),
                Descripcion = dr["Descripcion"].ToString(),
                DescripcionCorta = dr["DescripcionCorta"].ToString(),
                Precio = Convert.ToDecimal(dr["Precio"]),
                PrecioDescuento = dr["PrecioDescuento"] != DBNull.Value ? Convert.ToDecimal(dr["PrecioDescuento"]) : (decimal?)null,
                Stock = Convert.ToInt32(dr["Stock"]),
                Imagen = dr["Imagen"].ToString(),
                ImagenRuta = dr["ImagenRuta"].ToString(),
                CodigoProducto = dr["CodigoProducto"].ToString(),
                Peso = dr["Peso"] != DBNull.Value ? Convert.ToDecimal(dr["Peso"]) : (decimal?)null,
                Activo = Convert.ToBoolean(dr["Activo"]),
                ActivoCarrito = Convert.ToBoolean(dr["ActivoCarrito"]),
                Destacado = Convert.ToBoolean(dr["Destacado"]),
                Visitas = Convert.ToInt32(dr["Visitas"]),
                Calificacion = dr["Calificacion"] != DBNull.Value ? Convert.ToDecimal(dr["Calificacion"]) : (decimal?)null,
                FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                FechaActualizacion = Convert.ToDateTime(dr["FechaActualizacion"]),
                CategoriaNombre = dr["CategoriaNombre"].ToString()
            };
        }
    }
}
