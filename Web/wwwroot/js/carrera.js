// ========================================
// VARIABLES GLOBALES
// ========================================

let todasLasCarreras = [];
let paginaActual = 1;
let registrosPorPagina = 6;
let totalRegistros = 0;
let totalPaginas = 1;
let idEliminar = 0;

// URLs usando los handlers de Razor Pages
const API_URL_GET_ALL = (window.appBase || '') + '/Carrera?handler=GetAll';
const API_URL_INSTITUCIONES = (window.appBase || '') + '/Carrera?handler=Instituciones';
const API_URL_BUSCAR = (window.appBase || '') + '/Carrera?handler=Buscar';
const API_URL_CREAR = (window.appBase || '') + '/Carrera?handler=Crear';
const API_URL_EDITAR = (window.appBase || '') + '/Carrera?handler=Editar';
const API_URL_ELIMINAR = (window.appBase || '') + '/Carrera?handler=Eliminar';

// ========================================
// MODALES
// ========================================

function abrirModalCrear() {
    document.getElementById('tituloModal').textContent = 'Nueva Carrera';
    document.getElementById('idCarrera').value = '0';
    document.getElementById('nombre').value = '';
    document.getElementById('director').value = '';
    document.getElementById('email').value = '';
    document.getElementById('telefono').value = '';
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
// OBTENER TOKEN 
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
    const datos = todasLasCarreras.slice(inicio, inicio + registrosPorPagina);
    renderizarCarreras(datos);
    actualizarPaginacion();
}

// ========================================
// MOSTRAR CARRERAS
// ========================================

function renderizarCarreras(carreras) {
    const container = document.getElementById('listaCarreras');
    const paginacionContainer = document.getElementById('paginacionContainer');

    if (!carreras || carreras.length === 0) {
        if (todasLasCarreras.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-mortarboard" style="font-size: 48px; color: #6c757d;"></i>
                    <h3>No hay carreras registradas</h3>
                    <button class="btn btn-primary" onclick="abrirModalCrear()">
                        <i class="bi bi-plus-circle"></i> Crear primera carrera
                    </button>
                </div>
            `;
        } else {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="bi bi-inbox" style="font-size: 48px; color: #6c757d;"></i>
                    <p>No hay carreras en esta página</p>
                </div>
            `;
        }
        if (paginacionContainer) paginacionContainer.style.display = 'none';
        return;
    }

    let html = "";
    carreras.forEach(c => {
        const estado = c.activo ? 'Activo' : 'Inactivo';
        const estadoClass = c.activo ? 'bg-success' : 'bg-danger';
        const nombreEscapado = c.nombre.replace(/'/g, "\\'");

        html += `
            <div class="area-card">
                <div class="area-card-header">
                    <h3>${escapeHtml(c.nombre)}</h3>
                    <span class="badge ${estadoClass}">${estado}</span>
                </div>
                <div class="area-card-body">
                    <p><strong>ID:</strong> ${c.id}</p>
                    <p><strong>Director:</strong> ${escapeHtml(c.director || 'N/A')}</p>
                    <p><strong>Email:</strong> ${escapeHtml(c.email || 'N/A')}</p>
                    <p><strong>Teléfono:</strong> ${escapeHtml(c.telefono || 'N/A')}</p>
                    <p><strong>Institución:</strong> ${escapeHtml(c.institucionNombre || 'N/A')}</p>
                    <p><strong>ID Institución:</strong> ${c.institucionID}</p>
                    <p><strong>Fecha Creación:</strong> ${formatDate(c.fechaCreacion)}</p>
                    ${c.fechaModificacion ? `<p><strong>Última Modificación:</strong> ${formatDate(c.fechaModificacion)}</p>` : ''}
                </div>
                <div class="area-card-footer">
                    <button class="btn btn-sm btn-warning" onclick="editarCarrera(${c.id})">
                        <i class="bi bi-pencil"></i> Editar
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="abrirModalEliminar(${c.id}, '${nombreEscapado}')">
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
// CARGAR INSTITUCIONES
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
// CARGAR CARRERAS 
// ========================================

async function cargarCarreras() {
    mostrarLoading(true);

    try {
        const response = await fetch(API_URL_GET_ALL);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        console.log('Carreras cargadas:', result);

        todasLasCarreras = Array.isArray(result) ? result : (result.data || []);

        totalRegistros = todasLasCarreras.length;
        totalPaginas = Math.ceil(totalRegistros / registrosPorPagina) || 1;
        paginaActual = 1;

        mostrarPagina();
    } catch (e) {
        console.error('Error cargando carreras:', e);
        mostrarAlerta("Error cargando carreras: " + e.message, "danger");
        todasLasCarreras = [];
        renderizarCarreras([]);
        actualizarPaginacion();
    }

    mostrarLoading(false);
}

// ========================================
// BUSCAR 
// ========================================

async function buscarCarreraPorId() {
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
                    No se encontró una carrera con ID ${id}
                </div>
            `;
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        const c = result.data || result;

        resultadoDiv.innerHTML = `
            <div class="alert alert-success">
                <h5><i class="bi bi-check-circle"></i> Carrera encontrada</h5>
                <hr>
                <p><strong>ID:</strong> ${c.id}</p>
                <p><strong>Nombre:</strong> ${escapeHtml(c.nombre)}</p>
                <p><strong>Director:</strong> ${escapeHtml(c.director || 'N/A')}</p>
                <p><strong>Email:</strong> ${escapeHtml(c.email || 'N/A')}</p>
                <p><strong>Teléfono:</strong> ${escapeHtml(c.telefono || 'N/A')}</p>
                <p><strong>Institución:</strong> ${escapeHtml(c.institucionNombre || 'N/A')}</p>
                <p><strong>Estado:</strong> <span class="badge ${c.activo ? 'bg-success' : 'bg-danger'}">${c.activo ? 'Activo' : 'Inactivo'}</span></p>
                <p><strong>Fecha Creación:</strong> ${formatDate(c.fechaCreacion)}</p>
                ${c.fechaModificacion ? `<p><strong>Última Modificación:</strong> ${formatDate(c.fechaModificacion)}</p>` : ''}
                <button class="btn btn-sm btn-warning" onclick="editarCarrera(${c.id})">
                    <i class="bi bi-pencil"></i> Editar
                </button>
                <button class="btn btn-sm btn-danger" onclick="abrirModalEliminar(${c.id}, '${escapeHtml(c.nombre)}')">
                    <i class="bi bi-trash"></i> Eliminar
                </button>
                <button class="btn btn-sm btn-secondary" onclick="cerrarModalBuscar()">Cerrar</button>
            </div>
        `;
    } catch (e) {
        console.error('Error buscando carrera:', e);
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
    const director = document.getElementById('director');
    const email = document.getElementById('email');
    const telefono = document.getElementById('telefono');
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

    if (!director.value.trim()) {
        director.classList.add('is-invalid');
        document.getElementById('errDirector').textContent = 'El director es obligatorio';
        document.getElementById('errDirector').style.display = 'block';
        ok = false;
    }

    if (!email.value.trim()) {
        email.classList.add('is-invalid');
        document.getElementById('errEmail').textContent = 'El email es obligatorio';
        document.getElementById('errEmail').style.display = 'block';
        ok = false;
    } else {
        const emailRegex = /^[^\s@]+@(cuc\.ac\.cr|cuc\.cr)$/;
        if (!emailRegex.test(email.value.trim())) {
            email.classList.add('is-invalid');
            document.getElementById('errEmail').textContent = 'Solo se permiten @cuc.ac.cr o @cuc.cr';
            document.getElementById('errEmail').style.display = 'block';
            ok = false;
        }
    }

    if (!telefono.value.trim()) {
        telefono.classList.add('is-invalid');
        document.getElementById('errTelefono').textContent = 'El teléfono es obligatorio';
        document.getElementById('errTelefono').style.display = 'block';
        ok = false;
    } else if (!/^\d+$/.test(telefono.value.trim())) {
        telefono.classList.add('is-invalid');
        document.getElementById('errTelefono').textContent = 'Solo números';
        document.getElementById('errTelefono').style.display = 'block';
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
// GUARDAR 
// ========================================

async function guardarCarrera() {
    if (!validarFormulario()) return;

    const id = parseInt(document.getElementById('idCarrera').value) || 0;
    const nombre = document.getElementById('nombre').value.trim();
    const director = document.getElementById('director').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const institucionID = parseInt(document.getElementById('institucion').value);

    const data = {
        id: id,
        nombre: nombre,
        director: director,
        email: email,
        telefono: telefono,
        institucionID: institucionID,
        activo: true
    };

    const token = getAntiForgeryToken();
    mostrarLoading(true);

    try {
        let response;

        if (id === 0) {
            response = await fetch(API_URL_CREAR, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(data)
            });
        } else {
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
            await cargarCarreras();
            cerrarModalBuscar();
        } else {
            mostrarAlerta(result.mensaje || "Error al guardar la carrera", "danger");
        }
    } catch (e) {
        console.error('Error guardando carrera:', e);
        mostrarAlerta("Error guardando carrera: " + e.message, "danger");
    }

    mostrarLoading(false);
}

// ========================================
// EDITAR
// ========================================

async function editarCarrera(id) {
    mostrarLoading(true);

    try {
        const response = await fetch(`${API_URL_BUSCAR}&id=${id}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();
        const c = result.data || result;

        document.getElementById('idCarrera').value = c.id;
        document.getElementById('nombre').value = c.nombre;
        document.getElementById('director').value = c.director || '';
        document.getElementById('email').value = c.email || '';
        document.getElementById('telefono').value = c.telefono || '';
        document.getElementById('institucion').value = c.institucionID;
        document.getElementById('tituloModal').textContent = "Editar Carrera";

        limpiarErrores();
        document.getElementById('modalFormulario').style.display = 'flex';
    } catch (e) {
        console.error('Error cargando carrera para editar:', e);
        mostrarAlerta("Error cargando la carrera: " + e.message, "danger");
    }

    mostrarLoading(false);
}

// ========================================
// ELIMINAR 
// ========================================

async function eliminarCarrera() {
    if (idEliminar <= 0) {
        mostrarAlerta("ID de carrera no válido", "danger");
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
            await cargarCarreras();
            cerrarModalBuscar();
        } else {
            mostrarAlerta(result.mensaje || "Error al eliminar la carrera", "danger");
        }
    } catch (e) {
        console.error('Error eliminando carrera:', e);
        mostrarAlerta("Error eliminando carrera: " + e.message, "danger");
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
    console.log('Inicializando página de Carreras...');

    cargarInstituciones().then(() => {
        console.log('Instituciones cargadas, cargando carreras...');
        cargarCarreras();
    });
});