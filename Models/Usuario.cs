using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyec_Agricola_Web.Models
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellido_Paterno { get; set; }
        public string Apellido_Materno { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Contraseña { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public string Genero { get; set; }
        public int TipoUsuario { get; set; }

        //es el activo de la bd
        public bool Estado { get; set; }
    }
}