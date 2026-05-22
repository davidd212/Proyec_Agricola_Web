using System;
using System.Collections.Generic;

namespace Proyec_Agricola_Web.Models
{
    public class Pedido
    {
        public int PedidoID { get; set; }
        public int UsuarioID { get; set; }
        public string NumeroPedido { get; set; }
        public DateTime FechaPedido { get; set; }
        public DateTime? FechaEntrega { get; set; }

        public string DireccionEnvio { get; set; }
        public string CiudadEnvio { get; set; }
        public string CodigoPostalEnvio { get; set; }
        public string PaisEnvio { get; set; }
        public string TelefonoEnvio { get; set; }

        public string DireccionFacturacion { get; set; }
        public string CiudadFacturacion { get; set; }
        public string CodigoPostalFacturacion { get; set; }
        public string PaisFacturacion { get; set; }

        public decimal Subtotal { get; set; }
        public decimal ImpuestosTasa { get; set; }
        public decimal Impuestos { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Total { get; set; }

        public string Estado { get; set; }
        public string MetodoPago { get; set; }
        public string EstadoPago { get; set; }

        public string Notas { get; set; }
        public string Notas_Admin { get; set; }

        public string NumeroSeguimiento { get; set; }
        public string Transportista { get; set; }

        public DateTime FechaActualizacion { get; set; }
        public List<PedidoDetalle> DetalleItems { get; set; }

        public string NombreCliente { get; set; }

        public Pedido()
        {
            DetalleItems = new List<PedidoDetalle>();
        }
    }

    public class PedidoDetalle
    {
        public int PedidoDetalleID { get; set; }
        public int PedidoID { get; set; }
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public string NombreProducto { get; set; }
        public string ImagenProducto { get; set; }
    }
}
