using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Proyec_Agricola_Web.Models
{
    public class UsuarioDAL
    {
        // Usar cadena de conexión 'ConexionDB' en Web.config
        string conexion = ConfigurationManager.ConnectionStrings["ConexionDB"].ConnectionString;

        // Validar 
        public Usuario ValidarUsuario(string NombreUsuario, string contraseña)
        {
            Usuario user = null;

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Usuarios WHERE Nombre = @Nombre AND Contraseña = @Contraseña AND Activo = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", NombreUsuario);
                cmd.Parameters.AddWithValue("@Contraseña", contraseña);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        user = new Usuario
                        {
                            UsuarioID = Convert.ToInt32(dr["UsuarioID"]),
                            Nombre = dr["Nombre"].ToString(),
                            Apellido_Paterno = dr["Apellido_Paterno"].ToString(),
                            Apellido_Materno = dr["Apellido_Materno"].ToString(),
                            Email = dr["Email"].ToString(),
                            Telefono = dr["Telefono"].ToString(),
                            Contraseña = dr["Contraseña"].ToString(),
                            Direccion = dr["Direccion"].ToString(),
                            Ciudad = dr["Ciudad"].ToString(),
                            CodigoPostal = dr["CodigoPostal"].ToString(),
                            Genero = dr["Genero"].ToString(),
                            TipoUsuario = Convert.ToInt32(dr["TipoUsuario"]),
                            Estado = Convert.ToBoolean(dr["Activo"])
                        };
                    }
                }
            }
            return user;
        }

        public bool UsuarioExiste(string nombreUsuario, string Email)
        {
            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT COUNT(*) FROM Usuarios WHERE Nombre = @Nombre OR Email = @Email";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", nombreUsuario);
                cmd.Parameters.AddWithValue("@Email", Email);

                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // Registrar usuario 
        public List<Usuario> ObtenerTodosUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT * FROM Usuarios WHERE Activo = 1 ORDER BY Nombre";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            UsuarioID = Convert.ToInt32(dr["UsuarioID"]),
                            Nombre = dr["Nombre"].ToString(),
                            Apellido_Paterno = dr["Apellido_Paterno"].ToString(),
                            Apellido_Materno = dr["Apellido_Materno"].ToString(),
                            Email = dr["Email"].ToString(),
                            Telefono = dr["Telefono"].ToString(),
                            Direccion = dr["Direccion"].ToString(),
                            Ciudad = dr["Ciudad"].ToString(),
                            CodigoPostal = dr["CodigoPostal"].ToString(),
                            Genero = dr["Genero"].ToString(),
                            TipoUsuario = Convert.ToInt32(dr["TipoUsuario"]),
                            Estado = Convert.ToBoolean(dr["Activo"])
                        });
                    }
                }
            }

            return usuarios;
        }

        public List<Usuario> ObtenerUsuariosRecientes(int cantidad)
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection con = new SqlConnection(conexion))
            {
                string query = "SELECT TOP " + cantidad + " * FROM Usuarios WHERE Activo = 1 ORDER BY UsuarioID DESC";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            UsuarioID = Convert.ToInt32(dr["UsuarioID"]),
                            Nombre = dr["Nombre"].ToString(),
                            Apellido_Paterno = dr["Apellido_Paterno"].ToString(),
                            Apellido_Materno = dr["Apellido_Materno"].ToString(),
                            Email = dr["Email"].ToString(),
                            Telefono = dr["Telefono"].ToString(),
                            Direccion = dr["Direccion"].ToString(),
                            Ciudad = dr["Ciudad"].ToString(),
                            CodigoPostal = dr["CodigoPostal"].ToString(),
                            Genero = dr["Genero"].ToString(),
                            TipoUsuario = Convert.ToInt32(dr["TipoUsuario"]),
                            Estado = Convert.ToBoolean(dr["Activo"])
                        });
                    }
                }
            }

            return usuarios;
        }

        public bool RegistrarUsuario(Usuario usuario)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(conexion))
                {
                    string query = "INSERT INTO Usuarios (Nombre, Apellido_Paterno, Apellido_Materno, Email, Telefono, Contraseña, Direccion, Ciudad, CodigoPostal, Genero, TipoUsuario, Activo) VALUES (@Nombre, @Apellido_Paterno, @Apellido_Materno, @Email, @Telefono, @Contraseña, @Direccion, @Ciudad, @CodigoPostal, @Genero, @TipoUsuario, @Activo)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre ?? "");
                    cmd.Parameters.AddWithValue("@Apellido_Paterno", usuario.Apellido_Paterno ?? "");
                    cmd.Parameters.AddWithValue("@Apellido_Materno", usuario.Apellido_Materno ?? "");
                    cmd.Parameters.AddWithValue("@Email", usuario.Email ?? "");
                    cmd.Parameters.AddWithValue("@Telefono", usuario.Telefono ?? "");
                    cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña ?? "");
                    cmd.Parameters.AddWithValue("@Direccion", usuario.Direccion ?? "");
                    cmd.Parameters.AddWithValue("@Ciudad", usuario.Ciudad ?? "");
                    cmd.Parameters.AddWithValue("@CodigoPostal", usuario.CodigoPostal ?? "");
                    cmd.Parameters.AddWithValue("@Genero", usuario.Genero ?? "");
                    cmd.Parameters.AddWithValue("@TipoUsuario", usuario.TipoUsuario > 0 ? usuario.TipoUsuario : 2);
                    cmd.Parameters.AddWithValue("@Activo", 1);

                    con.Open();
                    int resultado = cmd.ExecuteNonQuery();
                    return resultado > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al registrar usuario: " + ex.Message);
                return false;
            }
        }
    }
}