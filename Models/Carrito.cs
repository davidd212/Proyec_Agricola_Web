using System;
using System.Collections.Generic;

namespace Proyec_Agricola_Web.Models
{
    public class Carrito
    {
        public int CarritoID { get; set; }
        public int? UsuarioID { get; set; }
        public string SesionID { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
        public bool Activo { get; set; }
        public List<CarritoDetalle> DetalleItems { get; set; }

        public Carrito()
        {
            DetalleItems = new List<CarritoDetalle>();
        }
    }

    public class CarritoDetalle
    {
        public int CarritoDetalleID { get; set; }
        public int CarritoID { get; set; }
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public DateTime FechaAgregado { get; set; }
        public string NombreProducto { get; set; }
        public string ImagenProducto { get; set; }
    }
}
