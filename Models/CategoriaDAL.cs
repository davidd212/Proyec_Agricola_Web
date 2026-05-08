using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Proyec_Agricola_Web.Models
{
    public class CategoriaDAL
    {
        string conexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

        // Obtener todas las categorías activas
        public List<Categoria> ObtenerTodasCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "SELECT CategoriaID, Nombre FROM Categorias WHERE Activa = 1 ORDER BY Nombre";

                    SqlCommand cmd = new SqlCommand(query, con);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            categorias.Add(new Categoria
                            {
                                CategoriaID = Convert.ToInt32(dr["CategoriaID"]),
                                Nombre = dr["Nombre"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al obtener categorías: " + ex.Message);
            }

            return categorias;
        }

        // Verificar si una categoría existe
        public bool CategoriaExiste(int categoriaID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "SELECT COUNT(*) FROM Categorias WHERE CategoriaID = @CategoriaID AND Activa = 1";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CategoriaID", categoriaID);

                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al verificar categoría: " + ex.Message);
                return false;
            }
        }
    }
}
