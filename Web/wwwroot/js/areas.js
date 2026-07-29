// ========================================
// VARIABLES GLOBALES
// ========================================

let todasLasAreas = [];
let paginaActual = 1;
let registrosPorPagina = 6;
let totalRegistros = 0;
let totalPaginas = 1;
let idEliminar = 0;

// URLs usando los handlers de Razor Pages
const API_URL_GET_ALL = '/Areas?handler=GetAll';
const API_URL_INSTITUCIONES = '/Areas?handler=Instituciones';
const API_URL_BUSCAR = '/Areas?handler=Buscar';
const API_URL_CREAR = '/Areas?handler=Crear';
const API_URL_EDITAR = '/Areas?handler=Editar';
const API_URL_ELIMINAR = '/Areas?handler=Eliminar';

// ========================================
// MODALES
// ========================================

function abrirModalCrear() {
    document.getElementById('tituloModal').textContent = 'Nueva Área';
    document.getElementById('idArea').value = '0';
    document.getElementById('nombre').value = '';
    document.getElementById('institucion').value = '';
    limpiarErrores();
    document.getElementById('modalFormulario').style.display = 'flex';
}

function cerrarModalFormulario() {
    document.getElementById('modalFormulario').style.display = 'none';
    limpiarErrores();
}

function abrirModalBuscar() {
    document.getElementById('buscarId').value = '';
    document.getElementById('buscarResultado').style.display = 'none';
    document.getElementById('buscarResultado').innerHTML = '';
    document.getElementById('modalBuscar').style.display = 'flex';
}

function cerrarModalBuscar() {
    document.getElementById('modalBuscar').style.display = 'none';
}

function abrirModalEliminar(id, nombre) {
    idEliminar = id;
    document.getElementById('nombreEliminar').textContent = nombre;
    document.getElementById('modalEliminar').style.display = 'flex';
}

function cerrarModalEliminar() {
    document.getElementById('modalEliminar').style.display = 'none';
}

// ========================================
// OBTENER TOKEN ANTIFORGERY
// ========================================

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

// ========================================
// ALERTAS
// ========================================

function mostrarAlerta(mensaje, tipo) {
    const div = document.getElementById('alertas');
    if (!div) return;

    const alertClass = tipo === 'success' ? 'alert-success' :
        tipo === 'warning' ? 'alert-warning' : 'alert-danger';

    const icon = tipo === 'success' ? 'bi-check-circle' :
        tipo === 'warning' ? 'bi-exclamation-triangle' : 'bi-exclamation-circle';

    div.innerHTML = `
        <div class="alert ${alertClass} alert-dismissible fade show">
            <i class="bi ${icon}"></i>
            ${mensaje}
            <button type="button" class="btn-close" onclick="this.parentElement.remove()">&times;</button>
        </div>
    `;

    setTimeout(() => {
        div.innerHTML = '';
    }, 5000);
}

function mostrarLoading(show) {
    const loading = document.getElementById('loading');
    if (loading) {
        loading.style.display = show ? 'flex' : 'none';
    }
}

// ========================================
// PAGINACION
// ========================================

function actualizarPaginacion() {
    const inicio = (paginaActual - 1) * registrosPorPagina + 1;
    const fin = Math.min(paginaActual * registrosPorPagina, totalRegistros);

    const inicioMostrar = document.getElementById('inicioMostrar');
    const finMostrar = document.getElementById('finMostrar');
    const total = document.getElementById('totalRegistros');
    const pagina = document.getElementById('numeroPagina');

    if (inicioMostrar) inicioMostrar.textContent = totalRegistros ? inicio : 0;
    if (finMostrar) finMostrar.textContent = totalRegistros ? fin : 0;
    if (total) total.textContent = totalRegistros;
    if (pagina) pagina.textContent = `${paginaActual} / ${totalPaginas}`;

    const btnAnterior = document.getElementById('btnAnterior');
    const btnSiguiente = document.getElementById('btnSiguiente');
    if (btnAnterior) btnAnterior.disabled = paginaActual <= 1;
    if (btnSiguiente) btnSiguiente.disabled = paginaActual >= totalPaginas;
}

function paginaAnterior() {
    if (paginaActual > 1) {
        paginaActual--;
        mostrarPagina();
    }
}

function paginaSiguiente() {
    if (paginaActual < totalPaginas) {
        paginaActual++;
        mostrarPagina();
    }
}

function mostrarPagina() {
    const inicio = (paginaActual - 1) * registrosPorPagina;
    const datos = todasLasAreas.slice(inicio, inicio + registrosPorPagina);
    renderizarAreas(datos);
    actualizarPaginacion();
}

// ========================================
// MOSTRAR AREAS
// ========================================

function renderizarAreas(areas) {
    const container = document.getElementById('listaAreas');
    const paginacionContainer = document.getElementById('paginacionContainer');

    if (!areas || areas.length === 0) {
        if (todasLasAreas.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-building" style="font-size: 48px; color: #6c757d;"></i>
                    <h3>No hay áreas registradas</h3>
                    <button class="btn btn-primary" onclick="abrirModalCrear()">
                        <i class="bi bi-plus-circle"></i> Crear primera área
                    </button>
                </div>
            `;
        } else {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-inbox" style="font-size: 48px; color: #6c757d;"></i>
                    <p>No hay áreas en esta página</p>
                </div>
            `;
        }
        if (paginacionContainer) paginacionContainer.style.display = 'none';
        return;
    }

    let html = "";
    areas.forEach(a => {
        const estado = a.activo ? 'Activo' : 'Inactivo';
        const estadoClass = a.activo ? 'bg-success' : 'bg-danger';
        const nombreEscapado = a.nombre.replace(/'/g, "\\'");

        html += `
            <div class="area-card">
                <div class="area-card-header">
                    <h3>${escapeHtml(a.nombre)}</h3>
                    <span class="badge ${estadoClass}">${estado}</span>
                </div>
                <div class="area-card-body">
                    <p><strong>ID:</strong> ${a.id}</p>
                    <p><strong>Institución:</strong> ${escapeHtml(a.institucionNombre || 'N/A')}</p>
                    <p><strong>ID Institución:</strong> ${a.institucionID}</p>
                    <p><strong>Fecha Creación:</strong> ${formatDate(a.fechaCreacion)}</p>
                    ${a.fechaModificacion ? `<p><strong>Última Modificación:</strong> ${formatDate(a.fechaModificacion)}</p>` : ''}
                </div>
                <div class="area-card-footer">
                    <button class="btn btn-sm btn-warning" onclick="editarArea(${a.id})">
                        <i class="bi bi-pencil"></i> Editar
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="abrirModalEliminar(${a.id}, '${nombreEscapado}')">
                        <i class="bi bi-trash"></i> Eliminar
                    </button>
                </div>
            </div>
        `;
    });

    container.innerHTML = html;
    if (paginacionContainer) paginacionContainer.style.display = 'block';
}

// ========================================
// CARGAR INSTITUCIONES - Usa el handler de Razor Pages
// ========================================

async function cargarInstituciones() {
    const select = document.getElementById('institucion');
    if (!select) return;

    select.innerHTML = '<option value="">Cargando instituciones...</option>';

    try {
        console.log('Cargando instituciones desde:', API_URL_INSTITUCIONES);

        const response = await fetch(API_URL_INSTITUCIONES);

        if (!response.ok) {
            throw new Error(`Error al cargar instituciones: ${response.status}`);
        }

        const data = await response.json();
        console.log('Instituciones cargadas:', data);

        // Verificar si la respuesta es un array o tiene la propiedad data
        const instituciones = Array.isArray(data) ? data : (data.data || data);

        select.innerHTML = '<option value="">Seleccione una institución...</option>';

        if (instituciones && instituciones.length > 0) {
            instituciones.forEach(function (inst) {
                const option = document.createElement('option');
                option.value = inst.id;
                option.textContent = `${inst.nombre} (ID: ${inst.id})`;
                select.appendChild(option);
            });
        } else {
            select.innerHTML = '<option value="">No hay instituciones disponibles</option>';
        }
    } catch (error) {
        console.error('Error cargando instituciones:', error);
        select.innerHTML = '<option value="">Error al cargar instituciones</option>';
        mostrarAlerta('Error al cargar lista de instituciones: ' + error.message, 'warning');
    }
}

// ========================================
// CARGAR AREAS - Usa el handler de Razor Pages
// ========================================

async function cargarAreas() {
    mostrarLoading(true);

    try {
        const response = await fetch(API_URL_GET_ALL);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log('Áreas cargadas:', result);

        todasLasAreas = Array.isArray(result) ? result : (result.data || []);

        totalRegistros = todasLasAreas.length;
        totalPaginas = Math.ceil(totalRegistros / registrosPorPagina) || 1;
        paginaActual = 1;

        mostrarPagina();
    } catch (e) {
        console.error('Error cargando áreas:', e);
        mostrarAlerta("Error cargando áreas: " + e.message, "danger");
        todasLasAreas = [];
        renderizarAreas([]);
        actualizarPaginacion();
    }

    mostrarLoading(false);
}

// ========================================
// BUSCAR - Usa el handler de Razor Pages
// ========================================

async function buscarAreaPorId() {
    const id = document.getElementById('buscarId').value;

    if (!id || id <= 0) {
        mostrarAlerta("Ingrese un ID válido mayor a 0", "warning");
        return;
    }

    const resultadoDiv = document.getElementById('buscarResultado');
    resultadoDiv.style.display = 'block';
    resultadoDiv.innerHTML = '<div class="loading"><div class="spinner-border"></div><p>Buscando...</p></div>';

    try {
        const response = await fetch(`${API_URL_BUSCAR}&id=${id}`);

        if (response.status === 404) {
            resultadoDiv.innerHTML = `
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle"></i>
                    No se encontró un área con ID ${id}
                </div>
            `;
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        const a = result.data || result;

        resultadoDiv.innerHTML = `
            <div class="alert alert-success">
                <h5><i class="bi bi-check-circle"></i> Área encontrada</h5>
                <hr>
                <p><strong>ID:</strong> ${a.id}</p>
                <p><strong>Nombre:</strong> ${escapeHtml(a.nombre)}</p>
                <p><strong>Institución:</strong> ${escapeHtml(a.institucionNombre || 'N/A')}</p>
                <p><strong>Estado:</strong> <span class="badge ${a.activo ? 'bg-success' : 'bg-danger'}">${a.activo ? 'Activo' : 'Inactivo'}</span></p>
                <p><strong>Fecha Creación:</strong> ${formatDate(a.fechaCreacion)}</p>
                ${a.fechaModificacion ? `<p><strong>Última Modificación:</strong> ${formatDate(a.fechaModificacion)}</p>` : ''}
                <button class="btn btn-sm btn-warning" onclick="editarArea(${a.id})">
                    <i class="bi bi-pencil"></i> Editar
                </button>
                <button class="btn btn-sm btn-danger" onclick="abrirModalEliminar(${a.id}, '${escapeHtml(a.nombre)}')">
                    <i class="bi bi-trash"></i> Eliminar
                </button>
                <button class="btn btn-sm btn-secondary" onclick="cerrarModalBuscar()">Cerrar</button>
            </div>
        `;
    } catch (e) {
        console.error('Error buscando área:', e);
        resultadoDiv.innerHTML = `
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-circle"></i>
                Error al buscar: ${escapeHtml(e.message)}
            </div>
        `;
    }
}

// ========================================
// VALIDACION
// ========================================

function limpiarErrores() {
    document.querySelectorAll('.is-invalid').forEach(x => x.classList.remove('is-invalid'));
    document.querySelectorAll('.invalid-feedback').forEach(x => x.style.display = 'none');
}

function validarFormulario() {
    let ok = true;
    limpiarErrores();

    const nombre = document.getElementById('nombre');
    const institucion = document.getElementById('institucion');

    if (!nombre.value.trim()) {
        nombre.classList.add('is-invalid');
        document.getElementById('errNombre').textContent = 'El nombre es obligatorio';
        document.getElementById('errNombre').style.display = 'block';
        ok = false;
    } else if (nombre.value.length > 100) {
        nombre.classList.add('is-invalid');
        document.getElementById('errNombre').textContent = 'El nombre no puede superar los 100 caracteres';
        document.getElementById('errNombre').style.display = 'block';
        ok = false;
    }

    if (!institucion.value || parseInt(institucion.value) <= 0) {
        institucion.classList.add('is-invalid');
        document.getElementById('errInstitucion').textContent = 'Debe seleccionar una institución';
        document.getElementById('errInstitucion').style.display = 'block';
        ok = false;
    }

    return ok;
}

// ========================================
// GUARDAR - Usa los handlers de Razor Pages
// ========================================

async function guardarArea() {
    if (!validarFormulario()) return;

    const id = parseInt(document.getElementById('idArea').value) || 0;
    const nombre = document.getElementById('nombre').value.trim();
    const institucionID = parseInt(document.getElementById('institucion').value);

    const data = {
        id: id,
        nombre: nombre,
        institucionID: institucionID,
        activo: true
    };

    const token = getAntiForgeryToken();
    mostrarLoading(true);

    try {
        let response;

        if (id === 0) {
            // Crear nueva área
            response = await fetch(API_URL_CREAR, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(data)
            });
        } else {
            // Editar área existente
            response = await fetch(`${API_URL_EDITAR}&id=${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(data)
            });
        }

        const result = await response.json();

        if (result.exito) {
            mostrarAlerta(result.mensaje, "success");
            cerrarModalFormulario();
            await cargarAreas();
            cerrarModalBuscar();
        } else {
            mostrarAlerta(result.mensaje || "Error al guardar el área", "danger");
        }
    } catch (e) {
        console.error('Error guardando área:', e);
        mostrarAlerta("Error guardando área: " + e.message, "danger");
    }

    mostrarLoading(false);
}

// ========================================
// EDITAR
// ========================================

async function editarArea(id) {
    mostrarLoading(true);

    try {
        const response = await fetch(`${API_URL_BUSCAR}&id=${id}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        const a = result.data || result;

        document.getElementById('idArea').value = a.id;
        document.getElementById('nombre').value = a.nombre;
        document.getElementById('institucion').value = a.institucionID;
        document.getElementById('tituloModal').textContent = "Editar Área";

        limpiarErrores();
        document.getElementById('modalFormulario').style.display = 'flex';
    } catch (e) {
        console.error('Error cargando área para editar:', e);
        mostrarAlerta("Error cargando el área: " + e.message, "danger");
    }

    mostrarLoading(false);
}

// ========================================
// ELIMINAR - Usa el handler de Razor Pages
// ========================================

async function eliminarArea() {
    if (idEliminar <= 0) {
        mostrarAlerta("ID de área no válido", "danger");
        return;
    }

    const token = getAntiForgeryToken();
    mostrarLoading(true);

    try {
        const response = await fetch(`${API_URL_ELIMINAR}&id=${idEliminar}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            }
        });

        const result = await response.json();

        if (result.exito) {
            mostrarAlerta(result.mensaje, "success");
            cerrarModalEliminar();
            await cargarAreas();
            cerrarModalBuscar();
        } else {
            mostrarAlerta(result.mensaje || "Error al eliminar el área", "danger");
        }
    } catch (e) {
        console.error('Error eliminando área:', e);
        mostrarAlerta("Error eliminando área: " + e.message, "danger");
    }

    mostrarLoading(false);
}

// ========================================
// UTILIDADES
// ========================================

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    try {
        const date = new Date(dateString);
        return date.toLocaleDateString('es-CR', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch {
        return dateString;
    }
}

// ========================================
// INICIO - Cargar al cargar la página
// ========================================

document.addEventListener("DOMContentLoaded", function () {
    console.log('Inicializando página de Áreas...');

    // Primero cargar las instituciones para el select
    cargarInstituciones().then(() => {
        console.log('Instituciones cargadas, cargando áreas...');
        cargarAreas();
    });
});