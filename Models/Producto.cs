using System;
using System.ComponentModel.DataAnnotations;

namespace Proyec_Agricola_Web.Models
{
    public class Producto
    {
        public int ProductoID { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaID { get; set; }

        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string DescripcionCorta { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "El precio con descuento debe ser válido")]
        public decimal? PrecioDescuento { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, 999999, ErrorMessage = "El stock debe ser válido")]
        public int Stock { get; set; }

        public string Imagen { get; set; }

        public string ImagenRuta { get; set; }

        public string CodigoProducto { get; set; }

        public decimal? Peso { get; set; }

        public bool Activo { get; set; }

        public bool ActivoCarrito { get; set; }

        public bool Destacado { get; set; }

        public int Visitas { get; set; }

        public decimal? Calificacion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaActualizacion { get; set; }

        public string CategoriaNombre { get; set; }
    }
}
