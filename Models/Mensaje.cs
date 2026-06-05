using System;

namespace Proyec_Agricola_Web.Models
{
    public class Mensaje
    {
        public int MensajeID { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Asunto { get; set; }
        public string MensajeTexto { get; set; }
        public DateTime FechaEnvio { get; set; }
        public bool Leido { get; set; }
        public DateTime? FechaLeido { get; set; }
    }
}