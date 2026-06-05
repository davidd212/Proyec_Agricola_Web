using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace Proyec_Agricola_Web.Models
{
    public class MensajeDAL
    {
        string conexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

        public bool InsertarMensaje(Mensaje mensaje)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = @"INSERT INTO Mensajes (Nombre, Email, Telefono, Asunto, MensajeTexto, FechaEnvio, Leido)
                                    VALUES (@Nombre, @Email, @Telefono, @Asunto, @MensajeTexto, GETDATE(), 0)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Nombre", mensaje.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@Email", mensaje.Email ?? "");
                    cmd.Parameters.AddWithValue("@Telefono", mensaje.Telefono ?? "");
                    cmd.Parameters.AddWithValue("@Asunto", mensaje.Asunto ?? "");
                    cmd.Parameters.AddWithValue("@MensajeTexto", mensaje.MensajeTexto ?? "");

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al insertar mensaje: " + ex.Message);
                return false;
            }
        }

        public List<Mensaje> ObtenerTodosMensajes()
        {
            List<Mensaje> mensajes = new List<Mensaje>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Mensajes ORDER BY FechaEnvio DESC";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        mensajes.Add(MapearMensaje(dr));
                    }
                }
            }

            return mensajes;
        }

        public Mensaje ObtenerMensajePorId(int mensajeID)
        {
            Mensaje mensaje = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Mensajes WHERE MensajeID = @MensajeID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@MensajeID", mensajeID);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        mensaje = MapearMensaje(dr);
                    }
                }
            }

            return mensaje;
        }

        public bool MarcarComoLeido(int mensajeID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "UPDATE Mensajes SET Leido = 1, FechaLeido = GETDATE() WHERE MensajeID = @MensajeID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MensajeID", mensajeID);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al marcar mensaje como leído: " + ex.Message);
                return false;
            }
        }

        public bool EliminarMensaje(int mensajeID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "DELETE FROM Mensajes WHERE MensajeID = @MensajeID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@MensajeID", mensajeID);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al eliminar mensaje: " + ex.Message);
                return false;
            }
        }

        public int ObtenerMensajesNoLeidosCount()
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT COUNT(*) FROM Mensajes WHERE Leido = 0";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private Mensaje MapearMensaje(SqlDataReader dr)
        {
            return new Mensaje
            {
                MensajeID = Convert.ToInt32(dr["MensajeID"]),
                Nombre = dr["Nombre"].ToString(),
                Email = dr["Email"].ToString(),
                Telefono = dr["Telefono"].ToString(),
                Asunto = dr["Asunto"].ToString(),
                MensajeTexto = dr["MensajeTexto"].ToString(),
                FechaEnvio = Convert.ToDateTime(dr["FechaEnvio"]),
                Leido = Convert.ToBoolean(dr["Leido"]),
                FechaLeido = dr["FechaLeido"] != DBNull.Value ? Convert.ToDateTime(dr["FechaLeido"]) : (DateTime?)null
            };
        }
    }
}