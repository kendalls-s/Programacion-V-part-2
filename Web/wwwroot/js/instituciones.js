// ========================================
// CONFIGURACIÓN
// ========================================
const API_URL = window.location.origin + '/institucion';
let idEliminar = 0;
let paginaActual = 1;
const registrosPorPagina = 15;
let totalRegistros = 0;
let totalPaginas = 0;
let todasLasInstituciones = [];

// ========================================
// FUNCIONES DE MODALES
// ========================================

function abrirModalCrear() {
    document.getElementById('tituloModal').textContent = 'Nueva Institución';
    document.getElementById('idInstitucion').value = '0';
    document.getElementById('nombre').value = '';
    document.getElementById('email').value = '';
    document.getElementById('telefono').value = '';
    document.getElementById('dominios').value = '';
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
// ALERTAS Y LOADING
// ========================================

function mostrarAlerta(mensaje, tipo) {
    const div = document.getElementById('alertas');
    div.innerHTML = `
        <div class="alert alert-${tipo} alert-dismissible">
            ${mensaje}
            <button onclick="this.parentElement.remove()" style="float:right; background:none; border:none; font-size:20px; cursor:pointer;">&times;</button>
        </div>
    `;
    setTimeout(() => { div.innerHTML = ''; }, 4000);
}

function mostrarLoading(show) {
    document.getElementById('loading').style.display = show ? 'block' : 'none';
    const paginacionContainer = document.getElementById('paginacionContainer');
    if (paginacionContainer) {
        paginacionContainer.style.display = show ? 'none' : 'flex';
    }
}

// ========================================
// PAGINACIÓN
// ========================================

function actualizarPaginacion() {
    const inicioMostrar = document.getElementById('inicioMostrar');
    const finMostrar = document.getElementById('finMostrar');
    const totalRegistrosSpan = document.getElementById('totalRegistros');
    const numeroPagina = document.getElementById('numeroPagina');
    const btnAnterior = document.getElementById('btnAnterior');
    const btnSiguiente = document.getElementById('btnSiguiente');
    const paginacionContainer = document.getElementById('paginacionContainer');

    if (totalRegistros > registrosPorPagina) {
        paginacionContainer.style.display = 'flex';
    } else {
        paginacionContainer.style.display = 'none';
    }

    const inicio = (paginaActual - 1) * registrosPorPagina + 1;
    const fin = Math.min(paginaActual * registrosPorPagina, totalRegistros);

    if (inicioMostrar) inicioMostrar.textContent = totalRegistros > 0 ? inicio : 0;
    if (finMostrar) finMostrar.textContent = totalRegistros > 0 ? fin : 0;
    if (totalRegistrosSpan) totalRegistrosSpan.textContent = totalRegistros;
    if (numeroPagina) numeroPagina.textContent = paginaActual;

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
    const fin = inicio + registrosPorPagina;
    const institucionesPagina = todasLasInstituciones.slice(inicio, fin);
    renderizarInstituciones(institucionesPagina);
    actualizarPaginacion();
}

function renderizarInstituciones(instituciones) {
    const container = document.getElementById('listaInstituciones');

    if (!instituciones || instituciones.length === 0) {
        if (todasLasInstituciones.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-university fa-4x"></i>
                    <p>No hay instituciones registradas</p>
                    <button class="btn btn-primary" onclick="abrirModalCrear()">
                        <i class="fas fa-plus"></i> Crear primera institución
                    </button>
                </div>
            `;
        } else {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-inbox fa-4x"></i>
                    <p>No hay instituciones en esta página</p>
                </div>
            `;
        }
        return;
    }

    let html = '';
    instituciones.forEach(function (i) {
        html += `
            <div class="institucion-card">
                <div class="institucion-card-header">
                    <h3>${i.nombre}</h3>
                    <span class="${i.activo ? 'badge-success' : 'badge-danger'}">${i.activo ? 'Activo' : 'Inactivo'}</span>
                </div>
                <div class="institucion-card-body">
                    <div class="info-row">
                        <span class="label"><i class="fas fa-id-badge"></i> ID:</span>
                        <span class="value">${i.id}</span>
                    </div>
                    <div class="info-row">
                        <span class="label"><i class="fas fa-envelope"></i> Email:</span>
                        <span class="value">${i.email}</span>
                    </div>
                    <div class="info-row">
                        <span class="label"><i class="fas fa-phone"></i> Teléfono:</span>
                        <span class="value">${i.telefono}</span>
                    </div>
                    <div class="info-row">
                        <span class="label"><i class="fas fa-globe"></i> Dominios:</span>
                        <span class="value">${i.dominios}</span>
                    </div>
                </div>
                <div class="institucion-card-footer">
                    <button class="btn-sm btn-info" onclick="editarInstitucion(${i.id})">
                        <i class="fas fa-edit"></i> Editar
                    </button>
                    <button class="btn-sm btn-danger" onclick="abrirModalEliminar(${i.id}, '${i.nombre}')">
                        <i class="fas fa-trash"></i> Eliminar
                    </button>
                </div>
            </div>
        `;
    });

    container.innerHTML = html;
}

// ========================================
// BUSCAR POR ID
// ========================================

async function buscarInstitucionPorId() {
    const id = document.getElementById('buscarId').value.trim();

    if (!id || parseInt(id) <= 0) {
        mostrarAlerta('Ingrese un ID válido mayor a 0', 'warning');
        return;
    }

    const resultadoDiv = document.getElementById('buscarResultado');
    resultadoDiv.style.display = 'block';
    resultadoDiv.innerHTML = '<div class="loading"><div class="spinner-border"></div><p>Buscando...</p></div>';

    try {
        const response = await fetch(`${API_URL}/${id}`);

        if (response.status === 404) {
            resultadoDiv.innerHTML = `
                <div class="alert alert-warning">
                    <i class="fas fa-exclamation-triangle"></i> No se encontró ninguna institución con ID ${id}
                </div>
            `;
            return;
        }

        if (!response.ok) {
            throw new Error('Error al buscar');
        }

        const i = await response.json();

        resultadoDiv.innerHTML = `
            <div style="background:#f8f9fa; padding:15px; border-radius:8px; border:1px solid #dee2e6;">
                <div style="display:grid; grid-template-columns:1fr 1fr; gap:8px; font-size:14px;">
                    <div><strong>ID:</strong> ${i.id}</div>
                    <div><strong>Nombre:</strong> ${i.nombre}</div>
                    <div><strong>Email:</strong> ${i.email}</div>
                    <div><strong>Teléfono:</strong> ${i.telefono}</div>
                    <div><strong>Dominios:</strong> ${i.dominios}</div>
                    <div><strong>Estado:</strong> <span class="${i.activo ? 'badge-success' : 'badge-danger'}">${i.activo ? 'Activo' : 'Inactivo'}</span></div>
                </div>
                <div style="margin-top:10px; display:flex; gap:8px;">
                    <button class="btn btn-warning btn-sm" onclick="editarInstitucion(${i.id})">Editar</button>
                    <button class="btn btn-danger btn-sm" onclick="abrirModalEliminar(${i.id}, '${i.nombre}')">Eliminar</button>
                    <button class="btn btn-secondary btn-sm" onclick="cerrarModalBuscar()">Cerrar</button>
                </div>
            </div>
        `;

    } catch (error) {
        resultadoDiv.innerHTML = `
            <div class="alert alert-danger">Error al buscar: ${error.message}</div>
        `;
    }
}

// ========================================
// VALIDACIONES
// ========================================

function limpiarErrores() {
    document.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
    document.querySelectorAll('.invalid-feedback').forEach(el => el.textContent = '');
}

function mostrarError(campo, mensaje) {
    const el = document.getElementById(campo);
    const err = document.getElementById('err' + campo.charAt(0).toUpperCase() + campo.slice(1));
    el.classList.add('is-invalid');
    err.textContent = mensaje;
}

function validarFormulario() {
    let valido = true;
    limpiarErrores();

    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const dominios = document.getElementById('dominios').value.trim();

    if (!nombre) { mostrarError('nombre', 'El nombre es requerido'); valido = false; }

    if (!email) {
        mostrarError('email', 'El email es requerido');
        valido = false;
    } else {
        const emailRegex = /^[^\s@]+@(cuc\.ac\.cr|cuc\.cr)$/;
        if (!emailRegex.test(email)) {
            mostrarError('email', 'Solo se permiten @cuc.ac.cr o @cuc.cr');
            valido = false;
        }
    }

    if (!telefono) {
        mostrarError('telefono', 'El teléfono es requerido');
        valido = false;
    } else if (!/^\d+$/.test(telefono)) {
        mostrarError('telefono', 'Solo números');
        valido = false;
    }

    if (!dominios) {
        mostrarError('dominios', 'Los dominios son requeridos');
        valido = false;
    } else {
        const dominiosValidos = ['cuc.ac.cr', 'cuc.cr'];
        const dominiosArray = dominios.split(',').map(d => d.trim().toLowerCase());
        const todosValidos = dominiosArray.every(d => dominiosValidos.includes(d));
        if (!todosValidos) {
            mostrarError('dominios', 'Solo se permiten: cuc.ac.cr, cuc.cr');
            valido = false;
        }
    }

    return valido;
}

function obtenerDatosFormulario() {
    return {
        nombre: document.getElementById('nombre').value.trim(),
        email: document.getElementById('email').value.trim(),
        telefono: document.getElementById('telefono').value.trim(),
        dominios: document.getElementById('dominios').value.trim()
    };
}

// ========================================
// CRUD - LISTAR TODOS
// ========================================

async function cargarInstituciones() {
    const container = document.getElementById('listaInstituciones');
    mostrarLoading(true);

    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error('Error al cargar');
        todasLasInstituciones = await response.json();

        totalRegistros = todasLasInstituciones.length;
        totalPaginas = Math.ceil(totalRegistros / registrosPorPagina);

        if (totalRegistros === 0) {
            paginaActual = 1;
            totalPaginas = 1;
            renderizarInstituciones([]);
            actualizarPaginacion();
            mostrarLoading(false);
            return;
        }

        if (paginaActual > totalPaginas) {
            paginaActual = totalPaginas;
        }

        mostrarPagina();

    } catch (error) {
        container.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-circle"></i> Error al cargar instituciones
            </div>
        `;
        document.getElementById('paginacionContainer').style.display = 'none';
    }

    mostrarLoading(false);
}

// ========================================
// CRUD - CREAR / EDITAR
// ========================================

async function guardarInstitucion() {
    if (!validarFormulario()) {
        return;
    }

    const id = Number(
        document.getElementById('idInstitucion').value
    );

    const isEdit = id > 0;
    const data = obtenerDatosFormulario();

    if (isEdit) {
        const institucionActual = todasLasInstituciones.find(
            i => Number(i.id ?? i.ID ?? i.Id) === id
        );

        data.id = id;
        data.activo = institucionActual
            ? (institucionActual.activo
                ?? institucionActual.Activo
                ?? true)
            : true;
    }

    const url = isEdit
        ? `${URL_API_INSTITUCION}/${id}`
        : URL_API_INSTITUCION;

    const method = isEdit ? 'PUT' : 'POST';

    console.log('Método:', method);
    console.log('URL:', url);
    console.log('Datos enviados:', data);

    const boton =
        document.getElementById('btnGuardarInstitucion');

    boton.disabled = true;

    try {
        const response = await fetch(url, {
            method: method,
            headers: construirHeaders(true),
            body: JSON.stringify(data)
        });

        if (manejarNoAutorizado(response)) {
            return;
        }

        if (!response.ok) {
            const mensaje = await obtenerMensajeError(
                response,
                `Error ${response.status} al guardar la institución`
            );

            console.error(
                'Respuesta del microservicio:',
                mensaje
            );

            mostrarAlerta(mensaje, 'danger');
            return;
        }

        cerrarModalFormulario();

        mostrarAlerta(
            isEdit
                ? 'Institución actualizada exitosamente'
                : 'Institución creada exitosamente',
            'success'
        );

        setTimeout(() => {
            window.location.reload();
        }, 600);

    } catch (error) {
        console.error('Error al guardar:', error);

        mostrarAlerta(
            'Error de conexión: ' + error.message,
            'danger'
        );
    } finally {
        boton.disabled = false;
    }
}

// ========================================
// CRUD - EDITAR (OBTENER POR ID)
// ========================================

async function editarInstitucion(id) {
    try {
        const response = await fetch(`${API_URL}/${id}`);
        if (!response.ok) throw new Error('Error al obtener datos');
        const i = await response.json();

        document.getElementById('tituloModal').textContent = 'Editar Institución';
        document.getElementById('idInstitucion').value = i.id;
        document.getElementById('nombre').value = i.nombre;
        document.getElementById('email').value = i.email;
        document.getElementById('telefono').value = i.telefono;
        document.getElementById('dominios').value = i.dominios;
        limpiarErrores();
        document.getElementById('modalFormulario').style.display = 'flex';
    } catch (error) {
        mostrarAlerta('Error al cargar datos: ' + error.message, 'danger');
    }
}

// ========================================
// CRUD - ELIMINAR
// ========================================

async function eliminarInstitucion() {
    try {
        const response = await fetch(`${API_URL}/${idEliminar}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            cerrarModalEliminar();
            mostrarAlerta('Institución eliminada exitosamente', 'success');
            await cargarInstituciones();
            cerrarModalBuscar();
        } else {
            const error = await response.json();
            mostrarAlerta(error.mensaje || error.error || 'Error al eliminar', 'danger');
        }
    } catch (error) {
        mostrarAlerta('Error de conexión: ' + error.message, 'danger');
    }
}

// ========================================
// INICIALIZAR
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    cargarInstituciones();
});