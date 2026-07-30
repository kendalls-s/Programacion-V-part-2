// ========================================
// BITÁCORA - JAVASCRIPT
// ========================================

let paginaActual = 1;
const registrosPorPagina = 15;

let totalRegistros = 0;
let totalPaginas = 0;

let registrosActuales = [];

let filtrosActuales = {
    fecha: '',
    usuario: '',
    accion: '',
    soloErrores: false
};

// ========================================
// INICIALIZACIÓN
// ========================================

document.addEventListener(
    'DOMContentLoaded',
    function () {
        cargarBitacoraConFiltros();
    }
);

// ========================================
// ALERTAS Y CARGA
// ========================================

function mostrarAlerta(mensaje, tipo = 'danger') {
    const contenedor =
        document.getElementById('alertas');

    if (!contenedor) {
        return;
    }

    contenedor.innerHTML = `
        <div class="alert alert-${tipo} alert-dismissible">

            <i class="bi bi-exclamation-circle"></i>

            ${escaparHtml(mensaje)}

            <button type="button"
                    class="btn-close"
                    onclick="this.parentElement.remove()">

                &times;

            </button>

        </div>
    `;

    setTimeout(function () {
        contenedor.innerHTML = '';
    }, 5000);
}

function limpiarAlertas() {
    const contenedor =
        document.getElementById('alertas');

    if (contenedor) {
        contenedor.innerHTML = '';
    }
}

function mostrarLoading(mostrar) {
    const loading =
        document.getElementById('loading');

    if (!loading) {
        return;
    }

    loading.style.display =
        mostrar ? 'block' : 'none';
}

// ========================================
// FILTROS
// ========================================

function aplicarFiltros() {
    filtrosActuales = {
        fecha:
            document
                .getElementById('filtroFecha')
                ?.value ?? '',

        usuario:
            document
                .getElementById('filtroUsuario')
                ?.value
                .trim() ?? '',

        accion:
            document
                .getElementById('filtroAccion')
                ?.value
                .trim() ?? '',

        soloErrores:
            document
                .getElementById('filtroSoloErrores')
                ?.checked ?? false
    };

    paginaActual = 1;

    cargarBitacoraConFiltros();
}

function limpiarFiltros() {
    const fecha =
        document.getElementById('filtroFecha');

    const usuario =
        document.getElementById('filtroUsuario');

    const accion =
        document.getElementById('filtroAccion');

    const soloErrores =
        document.getElementById(
            'filtroSoloErrores'
        );

    if (fecha) {
        fecha.value = '';
    }

    if (usuario) {
        usuario.value = '';
    }

    if (accion) {
        accion.value = '';
    }

    if (soloErrores) {
        soloErrores.checked = false;
    }

    filtrosActuales = {
        fecha: '',
        usuario: '',
        accion: '',
        soloErrores: false
    };

    paginaActual = 1;

    cargarBitacoraConFiltros();
}

function cargarBitacora() {
    cargarBitacoraConFiltros();
}

// ========================================
// CONSULTAR BITÁCORA
// ========================================

async function cargarBitacoraConFiltros() {
    const contenedor =
        document.getElementById('listaBitacora');

    if (!contenedor) {
        return;
    }

    limpiarAlertas();
    mostrarLoading(true);

    try {
        const parametros =
            new URLSearchParams();

        parametros.set(
            'pagina',
            paginaActual.toString()
        );

        parametros.set(
            'tamanoPagina',
            registrosPorPagina.toString()
        );

        parametros.set(
            'soloErrores',
            filtrosActuales.soloErrores.toString()
        );

        if (filtrosActuales.fecha) {
            parametros.set(
                'fecha',
                filtrosActuales.fecha
            );
        }

        if (filtrosActuales.usuario) {
            parametros.set(
                'usuario',
                filtrosActuales.usuario
            );
        }

        if (filtrosActuales.accion) {
            parametros.set(
                'accion',
                filtrosActuales.accion
            );
        }

        const url =
            `${window.appBase}/Bitacora?handler=Filtros&${parametros.toString()}`;

        const response = await fetch(
            url,
            {
                method: 'GET',

                headers: {
                    Accept: 'application/json'
                },

                credentials: 'same-origin'
            }
        );

        const texto =
            await response.text();

        let data = null;

        try {
            data =
                texto
                    ? JSON.parse(texto)
                    : null;
        }
        catch {
            console.error(
                'Respuesta recibida:',
                texto
            );

            throw new Error(
                'El servidor devolvió una respuesta inválida.'
            );
        }

        if (response.status === 401) {
            window.location.href = window.appBase + '/Login';
            return;
        }

        if (!response.ok) {
            throw new Error(
                data?.mensaje ??
                data?.Mensaje ??
                data?.message ??
                data?.Message ??
                data?.detalle ??
                data?.Detalle ??
                `Error HTTP ${response.status}`
            );
        }

        procesarRespuesta(data);
    }
    catch (error) {
        console.error(
            'Error al cargar la bitácora:',
            error
        );

        contenedor.innerHTML = `
            <div class="empty-state">

                <i class="bi bi-exclamation-circle"
                   style="
                       font-size: 60px;
                       color: #dc3545;
                   ">
                </i>

                <p>
                    No fue posible cargar la bitácora.
                </p>

            </div>
        `;

        mostrarAlerta(
            error.message ??
            'No fue posible cargar la bitácora.',
            'danger'
        );

        ocultarPaginacion();
    }
    finally {
        mostrarLoading(false);
    }
}

function procesarRespuesta(data) {
    registrosActuales =
        data?.registros ??
        data?.Registros ??
        data?.items ??
        data?.Items ??
        data?.data ??
        data?.Data ??
        [];

    if (!Array.isArray(registrosActuales)) {
        registrosActuales = [];
    }

    totalRegistros = Number(
        data?.totalRegistros ??
        data?.TotalRegistros ??
        registrosActuales.length
    );

    totalPaginas = Number(
        data?.totalPaginas ??
        data?.TotalPaginas ??
        Math.ceil(
            totalRegistros /
            registrosPorPagina
        )
    );

    paginaActual = Number(
        data?.paginaActual ??
        data?.PaginaActual ??
        data?.pagina ??
        data?.Pagina ??
        paginaActual
    );

    if (Number.isNaN(totalRegistros)) {
        totalRegistros =
            registrosActuales.length;
    }

    if (
        Number.isNaN(totalPaginas) ||
        totalPaginas < 0
    ) {
        totalPaginas = 0;
    }

    if (
        Number.isNaN(paginaActual) ||
        paginaActual < 1
    ) {
        paginaActual = 1;
    }

    mostrarRegistros(registrosActuales);
    actualizarPaginacion();
}

// ========================================
// MOSTRAR REGISTROS
// ========================================

function mostrarRegistros(registros) {
    const contenedor =
        document.getElementById('listaBitacora');

    if (!contenedor) {
        return;
    }

    if (
        !Array.isArray(registros) ||
        registros.length === 0
    ) {
        contenedor.innerHTML = `
            <div class="empty-state">

                <i class="bi bi-clock-history"
                   style="font-size: 64px;">
                </i>

                <p>
                    No hay registros en la bitácora que
                    coincidan con los filtros.
                </p>

            </div>
        `;

        ocultarPaginacion();

        return;
    }

    contenedor.innerHTML =
        registros
            .map(function (registro, indice) {
                return crearTarjeta(
                    registro,
                    indice
                );
            })
            .join('');
}

function crearTarjeta(registro, indice) {
    const id =
        registro?.id ??
        registro?.Id ??
        registro?.ID ??
        indice;

    const usuario =
        registro?.usuario ??
        registro?.Usuario ??
        'Sin usuario';

    const accion =
        registro?.accion ??
        registro?.Accion ??
        'Sin descripción';

    const detalleJson =
        registro?.detalleJson ??
        registro?.DetalleJson ??
        registro?.detalle ??
        registro?.Detalle ??
        '';

    const fecha =
        registro?.fecha ??
        registro?.Fecha ??
        null;

    const esError =
        convertirBooleano(
            registro?.esError ??
            registro?.EsError ??
            false
        );

    const fechaFormateada =
        formatearFecha(fecha);

    const claseBadge =
        obtenerClaseBadge(
            accion,
            esError
        );

    const iconoError =
        esError
            ? `
                <i class="bi bi-exclamation-triangle-fill"
                   style="
                       color: #dc3545;
                       margin-right: 5px;
                   ">
                </i>
              `
            : '';

    let accionCompleta =
        String(accion);

    if (detalleJson) {
        accionCompleta +=
            '\n\n' +
            formatearDetalleTexto(
                detalleJson
            );
    }

    return `
        <div class="card-bordered">

            <div class="card-bordered-header">

                <h3>
                    <i class="bi bi-person"></i>
                    ${escaparHtml(usuario)}
                </h3>

                <span class="${claseBadge}">
                    ${escaparHtml(fechaFormateada)}
                </span>

            </div>

            <div class="card-bordered-body">

                <div class="info-row">

                    <span class="label">

                        <i class="bi bi-info-circle"></i>
                        Acción:

                    </span>

                    <span class="value">

                        ${iconoError}

                        <pre class="json-bitacora">${escaparHtml(accionCompleta)}</pre>

                    </span>

                </div>

            </div>

            <div class="card-bordered-footer">

                <button type="button"
                        class="btn btn-info btn-sm"
                        onclick="abrirModalDetalle(${indice})">

                    <i class="bi bi-eye"></i>
                    Ver detalle

                </button>

            </div>

        </div>
    `;
}

function obtenerClaseBadge(
    accion,
    esError
) {
    if (esError) {
        return 'badge-items-danger';
    }

    const texto =
        String(accion ?? '')
            .toLowerCase();

    if (
        texto.includes('creó') ||
        texto.includes('creado') ||
        texto.includes('crear') ||
        texto.includes('registró') ||
        texto.includes('registrado') ||
        texto.includes('nuevo registro')
    ) {
        return 'badge-items-success';
    }

    if (
        texto.includes('eliminó') ||
        texto.includes('eliminado') ||
        texto.includes('eliminar')
    ) {
        return 'badge-items-danger';
    }

    if (
        texto.includes('modificó') ||
        texto.includes('modificado') ||
        texto.includes('modificar') ||
        texto.includes('actualizó') ||
        texto.includes('actualizado') ||
        texto.includes('actualizar')
    ) {
        return 'badge-items-warning';
    }

    if (
        texto.includes('consulta') ||
        texto.includes('consultó') ||
        texto.includes('consultar')
    ) {
        return 'badge-items-info';
    }

    return 'badge-items';
}

// ========================================
// MODAL DETALLE
// ========================================

function abrirModalDetalle(indice) {
    const registro =
        registrosActuales[indice];

    if (!registro) {
        mostrarAlerta(
            'No se encontró el registro seleccionado.',
            'danger'
        );

        return;
    }

    const contenido =
        document.getElementById(
            'detalleContenido'
        );

    const modal =
        document.getElementById(
            'modalDetalle'
        );

    if (!contenido || !modal) {
        return;
    }

    const id =
        registro?.id ??
        registro?.Id ??
        registro?.ID ??
        '';

    const usuario =
        registro?.usuario ??
        registro?.Usuario ??
        'Sin usuario';

    const accion =
        registro?.accion ??
        registro?.Accion ??
        'Sin descripción';

    const detalleJson =
        registro?.detalleJson ??
        registro?.DetalleJson ??
        registro?.detalle ??
        registro?.Detalle ??
        '';

    const fecha =
        registro?.fecha ??
        registro?.Fecha ??
        null;

    const esError =
        convertirBooleano(
            registro?.esError ??
            registro?.EsError ??
            false
        );

    const badgeEstado =
        esError
            ? `
                <span class="badge-items-danger">
                    ERROR
                </span>
              `
            : `
                <span class="badge-items-success">
                    EVENTO
                </span>
              `;

    contenido.innerHTML = `
        <div style="
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            font-size: 14px;
        ">

            <div>

                <strong>
                    ID:
                </strong>

                <div style="margin-top: 5px;">
                    ${escaparHtml(id)}
                </div>

            </div>

            <div>

                <strong>
                    Tipo:
                </strong>

                <div style="margin-top: 5px;">
                    ${badgeEstado}
                </div>

            </div>

            <div style="grid-column: span 2;">

                <strong>
                    Usuario:
                </strong>

                <div style="margin-top: 5px;">

                    <span class="badge-items">
                        ${escaparHtml(usuario)}
                    </span>

                </div>

            </div>

            <div style="grid-column: span 2;">

                <strong>
                    Acción:
                </strong>

                <div style="
                    margin-top: 5px;
                    padding: 12px;
                    background: #f8f9fa;
                    border: 1px solid #dee2e6;
                    border-radius: 8px;
                ">

                    ${escaparHtml(accion)}

                </div>

            </div>

            <div style="grid-column: span 2;">

                <strong>
                    Detalle:
                </strong>

                <pre class="json-bitacora"
                     style="margin-top: 5px;">${formatearDetalleHtml(detalleJson)}</pre>

            </div>

            <div style="grid-column: span 2;">

                <strong>
                    Fecha:
                </strong>

                <div style="margin-top: 5px;">
                    ${escaparHtml(
        formatearFechaCompleta(fecha)
    )}
                </div>

            </div>

        </div>
    `;

    modal.classList.add('show');
}

function cerrarModalDetalle() {
    const modal =
        document.getElementById(
            'modalDetalle'
        );

    if (!modal) {
        return;
    }

    modal.classList.remove('show');
}

window.addEventListener(
    'click',
    function (evento) {
        const modal =
            document.getElementById(
                'modalDetalle'
            );

        if (evento.target === modal) {
            cerrarModalDetalle();
        }
    }
);

// ========================================
// PAGINACIÓN
// ========================================

function actualizarPaginacion() {
    const contenedor =
        document.getElementById(
            'paginacionContainer'
        );

    if (!contenedor) {
        return;
    }

    if (
        totalRegistros <= 0 ||
        totalPaginas <= 1
    ) {
        contenedor.style.display = 'none';
        return;
    }

    contenedor.style.display = 'block';

    const inicio =
        ((paginaActual - 1) *
            registrosPorPagina) + 1;

    const fin =
        Math.min(
            paginaActual *
            registrosPorPagina,
            totalRegistros
        );

    document.getElementById(
        'inicioMostrar'
    ).textContent = inicio.toString();

    document.getElementById(
        'finMostrar'
    ).textContent = fin.toString();

    document.getElementById(
        'totalRegistros'
    ).textContent =
        totalRegistros.toString();

    document.getElementById(
        'numeroPagina'
    ).textContent =
        `${paginaActual} de ${totalPaginas}`;

    document.getElementById(
        'btnAnterior'
    ).disabled =
        paginaActual <= 1;

    document.getElementById(
        'btnSiguiente'
    ).disabled =
        paginaActual >= totalPaginas;
}

function ocultarPaginacion() {
    const contenedor =
        document.getElementById(
            'paginacionContainer'
        );

    if (contenedor) {
        contenedor.style.display = 'none';
    }
}

function paginaAnterior() {
    if (paginaActual <= 1) {
        return;
    }

    paginaActual--;

    cargarBitacoraConFiltros();
}

function paginaSiguiente() {
    if (
        paginaActual >= totalPaginas
    ) {
        return;
    }

    paginaActual++;

    cargarBitacoraConFiltros();
}

// ========================================
// UTILIDADES
// ========================================

function convertirBooleano(valor) {
    return (
        valor === true ||
        valor === 'true' ||
        valor === 'True' ||
        valor === 1 ||
        valor === '1'
    );
}

function formatearFecha(fecha) {
    if (!fecha) {
        return 'Sin fecha';
    }

    const fechaConvertida =
        new Date(fecha);

    if (
        Number.isNaN(
            fechaConvertida.getTime()
        )
    ) {
        return String(fecha);
    }

    return fechaConvertida.toLocaleString(
        'es-CR',
        {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }
    );
}

function formatearFechaCompleta(fecha) {
    if (!fecha) {
        return 'Sin fecha';
    }

    const fechaConvertida =
        new Date(fecha);

    if (
        Number.isNaN(
            fechaConvertida.getTime()
        )
    ) {
        return String(fecha);
    }

    return fechaConvertida.toLocaleString(
        'es-CR',
        {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        }
    );
}

function formatearDetalleTexto(detalle) {
    if (!detalle) {
        return '';
    }

    if (typeof detalle === 'object') {
        return JSON.stringify(
            detalle,
            null,
            2
        );
    }

    try {
        const objeto =
            JSON.parse(detalle);

        return JSON.stringify(
            objeto,
            null,
            2
        );
    }
    catch {
        return String(detalle);
    }
}

function formatearDetalleHtml(detalle) {
    if (!detalle) {
        return 'Sin detalle';
    }

    const texto =
        formatearDetalleTexto(detalle);

    return escaparHtml(texto);
}

function escaparHtml(valor) {
    return String(valor ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}