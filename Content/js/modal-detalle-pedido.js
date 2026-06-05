/**
 * =====================================================
 * JAVASCRIPT PARA MODAL DE DETALLES DE PEDIDO
 * =====================================================
 * Maneja la apertura, carga y visualización de detalles
 */

// Objeto global para gestionar el modal
var ModalDetallePedido = {

    /**
     * Inicializar el módulo
     */
    init: function() {
        this.cacheElements();
        this.bindEvents();
    },

    /**
     * Guardar referencias a elementos del DOM
     */
    cacheElements: function() {
        this.$modal = $('#modalDetallePedido');
        this.$btnDescargar = $('#btnDescargarPedido');
        this.$productosTable = $('#modalProductosTable tbody');
        this.pedidoActual = null;
    },

    /**
     * Vincular eventos
     */
    bindEvents: function() {
        var self = this;

        // Botón descargar PDF
        this.$btnDescargar.on('click', function() {
            if (self.pedidoActual) {
                self.descargarPDF(self.pedidoActual.PedidoID);
            }
        });

        // Cerrar modal (Bootstrap 5)
        this.$modal.on('hidden.bs.modal', function() {
            self.limpiarModal();
        });
    },

    /**
     * Abrir modal con detalles del pedido
     * @param {int} pedidoID - ID del pedido a mostrar
     */
    abrirModal: function(pedidoID) {
        var self = this;

        // Mostrar spinner de carga
        this.mostrarCarga();

        // Obtener datos del pedido
        $.ajax({
            url: '/Pedido/ObtenerDetallePedido',
            type: 'GET',
            data: { id: pedidoID },
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    self.pedidoActual = response.data;
                    self.llenarModal(response.data);
                    // Bootstrap 5 API
                    var modal = new bootstrap.Modal(self.$modal[0]);
                    modal.show();
                } else {
                    self.mostrarError('No se pudo cargar el pedido: ' + response.message);
                }
            },
            error: function(xhr, status, error) {
                self.mostrarError('Error al cargar el pedido: ' + error);
            },
            complete: function() {
                self.ocultarCarga();
            }
        });
    },

    /**
     * Llenar el modal con los datos del pedido
     * @param {object} pedido - Objeto con datos del pedido
     */
    llenarModal: function(pedido) {
        // Información general del pedido
        $('#modalNumeroPedido').text('#' + (pedido.NumeroPedido || 'N/A'));
        $('#modalFechaPedido').text(this.formatearFecha(pedido.FechaPedido));
        $('#modalFechaActualizacion').text(this.formatearFecha(pedido.FechaActualizacion));

        // Estado del pedido
        var badgeHTML = this.obtenerBadgeEstado(pedido.Estado);
        $('#modalEstado').html(badgeHTML);

        // Información de envío
        $('#modalDireccionEnvio').text(pedido.DireccionEnvio || 'N/A');
        $('#modalCiudadEnvio').text(pedido.CiudadEnvio || 'N/A');
        $('#modalCodigoPostalEnvio').text(pedido.CodigoPostalEnvio || 'N/A');
        $('#modalPaisEnvio').text(pedido.PaisEnvio || 'México');
        $('#modalTelefonoEnvio').text(pedido.TelefonoEnvio || 'N/A');

        // Resumen de precios
        $('#modalSubtotal').text(this.formatearMoneda(pedido.Subtotal));
        $('#modalImpuestos').text(this.formatearMoneda(pedido.Impuestos || 0));
        $('#modalCostoEnvio').html('<span style="color: #28a745; font-weight: 700;">GRATIS</span>');
        $('#modalTotal').text(this.formatearMoneda(pedido.Total));

        // Llenar tabla de productos
        this.llenarTablaProductos(pedido.DetalleItems);
    },

    /**
     * Llenar tabla de productos
     * @param {array} items - Array de items del pedido
     */
    llenarTablaProductos: function(items) {
        var self = this;
        this.$productosTable.empty();

        if (!items || items.length === 0) {
            this.$productosTable.append(
                '<tr><td colspan="4" class="text-center text-muted">No hay productos en este pedido</td></tr>'
            );
            return;
        }

        items.forEach(function(item, index) {
            var html = `
                <tr class="item-producto-${index}">
                    <td>
                        <div style="display: flex; align-items: center;">
                            ${item.ImagenProducto ? '<img src="' + item.ImagenProducto + '" style="width: 40px; height: 40px; margin-right: 10px; border-radius: 4px; object-fit: cover;" alt="' + item.NombreProducto + '">' : '<i class="fas fa-box" style="font-size: 1.5rem; color: #ccc; margin-right: 10px;"></i>'}
                            <span>${item.NombreProducto || 'Producto N/A'}</span>
                        </div>
                    </td>
                    <td class="text-center">${item.Cantidad}</td>
                    <td class="text-center">${self.formatearMoneda(item.PrecioUnitario)}</td>
                    <td class="text-right"><strong>${self.formatearMoneda(item.PrecioTotal)}</strong></td>
                </tr>
            `;
            self.$productosTable.append(html);
        });
    },

    /**
     * Obtener HTML del badge de estado
     * @param {string} estado - Estado del pedido
     * @returns {string} HTML del badge
     */
    obtenerBadgeEstado: function(estado) {
        var clases = {
            'Pendiente': 'bg-warning text-dark',
            'Procesando': 'bg-info',
            'Enviado': 'bg-primary',
            'Entregado': 'bg-success',
            'Cancelado': 'bg-danger'
        };

        var clase = clases[estado] || 'bg-secondary';
        return `<span class="badge ${clase}">${estado || 'Desconocido'}</span>`;
    },

    /**
     * Formatear fecha a formato legible
     * @param {string} fecha - Fecha en formato ISO
     * @returns {string} Fecha formateada
     */
    formatearFecha: function(fecha) {
        if (!fecha) return 'N/A';

        var options = {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        };

        try {
            return new Date(fecha).toLocaleDateString('es-MX', options);
        } catch (e) {
            return fecha;
        }
    },

    /**
     * Formatear número como moneda
     * @param {number} cantidad - Cantidad a formatear
     * @returns {string} Cantidad formateada
     */
    formatearMoneda: function(cantidad) {
        if (!cantidad) cantidad = 0;
        return new Intl.NumberFormat('es-MX', {
            style: 'currency',
            currency: 'MXN'
        }).format(cantidad);
    },

    /**
     * Descargar PDF del pedido
     * @param {int} pedidoID - ID del pedido
     */
    descargarPDF: function(pedidoID) {
        var self = this;

        $.ajax({
            url: '/Pedido/DescargarPDF',
            type: 'GET',
            data: { id: pedidoID },
            xhrFields: {
                responseType: 'blob'
            },
            success: function(blob) {
                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = 'pedido_' + pedidoID + '.pdf';
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);

                self.mostrarExito('PDF descargado correctamente');
            },
            error: function(xhr, status, error) {
                self.mostrarError('Error al descargar el PDF: ' + error);
            }
        });
    },

    /**
     * Mostrar animación de carga como overlay sin destruir el contenido
     */
    mostrarCarga: function() {
        if (this.$modal.find('.modal-loading-overlay').length === 0) {
            var overlay = `
                <div class="modal-loading-overlay" style="position: absolute; top: 0; left: 0; width: 100%; height: 100%; background: rgba(255,255,255,0.8); display: flex; align-items: center; justify-content: center; z-index: 1050; border-radius: 0 0 12px 12px;">
                    <div class="spinner-border" style="width: 3rem; height: 3rem; border-width: 4px;"></div>
                </div>
            `;
            this.$modal.find('.modal-body').css('position', 'relative').append(overlay);
        }
    },

    /**
     * Ocultar animación de carga
     */
    ocultarCarga: function() {
        this.$modal.find('.modal-loading-overlay').remove();
        this.$modal.find('.modal-body').css('position', '');
    },

    /**
     * Limpiar modal
     */
    limpiarModal: function() {
        this.pedidoActual = null;
        this.$productosTable.empty();
    },

    /**
     * Mostrar mensaje de error
     * @param {string} mensaje - Mensaje a mostrar
     */
    mostrarError: function(mensaje) {
        var alertHTML = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-circle"></i> ${mensaje}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
                </button>
            </div>
        `;

        this.$modal.find('.modal-body').prepend(alertHTML);

        setTimeout(function() {
            this.$modal.find('.alert').fadeOut(function() {
                $(this).remove();
            });
        }.bind(this), 5000);
    },

    /**
     * Mostrar mensaje de éxito
     * @param {string} mensaje - Mensaje a mostrar
     */
    mostrarExito: function(mensaje) {
        var alertHTML = `
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fas fa-check-circle"></i> ${mensaje}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
                </button>
            </div>
        `;

        var $alert = $(alertHTML);
        this.$modal.find('.modal-body').prepend($alert);

        setTimeout(function() {
            $alert.fadeOut(function() {
                $(this).remove();
            });
        }, 5000);
    }
};

/**
 * Función de ayuda para abrir modal desde HTML
 * @param {int} pedidoID - ID del pedido
 */
function abrirDetallePedido(pedidoID) {
    ModalDetallePedido.abrirModal(pedidoID);
}

/**
 * Inicializar cuando el documento esté listo
 */
$(document).ready(function() {
    ModalDetallePedido.init();
});

/**
 * Ejemplo de uso en HTML:
 * <button class="btn btn-sm btn-outline-success" onclick="abrirDetallePedido(123)">
 *     <i class="fas fa-eye"></i> Ver detalle
 * </button>
 */
