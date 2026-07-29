// ========================================
// TIPOS DE USUARIO - JAVASCRIPT
// ========================================

let idEliminar = 0;
let paginaActual = 1;
const registrosPorPagina = 15;
let todosLosTipos = [];

// ========================================
// DATOS INICIALES
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    const storedToken = localStorage.getItem('accessToken');

    if (!storedToken) {
        localStorage.setItem('redirectMessage', 'Por favor inicie sesión para utilizar el sistema');
        window.location.href = '/Login';
        return;
    }

    cargarTiposUsuario();
});

// ========================================
// CONFIGURACIÓN
// ========================================

const API_URL = 'https://localhost:7020/api/TipoUsuario';

function getToken() {
    return localStorage.getItem('accessToken');
}

function getHeaders() {
    const token = getToken();
    if (!token) {
        mostrarAlerta('No hay sesión activa. Redirigiendo...', 'warning');
        setTimeout(function () {
            window.location.href = '/Login';
        }, 1500);
        return null;
    }
    return {
        'Authorization': 'Bearer ' + token,
        'Content-Type': 'application/json'
    };
}

function handleUnauthorized(response) {
    if (response.status === 401) {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('nombreCompleto');
        localStorage.removeItem('usuarioTipo');
        localStorage.removeItem('usuarioId');
        localStorage.setItem('redirectMessage', 'Su sesión ha expirado. Por favor inicie sesión nuevamente.');
        window.location.href = '/Login';
        return true;
    }
    return false;
}

// ========================================
// ALERTAS Y LOADING
// ========================================

function mostrarAlerta(mensaje, tipo) {
    const contenedor = document.getElementById('alertas');
    if (!contenedor) {
        console.error(mensaje);
        return;
    }

    contenedor.innerHTML = `
        <div class="alert alert-${tipo} alert-dismissible">
            ${mensaje}
            <button type="button" class="btn-close" onclick="this.parentElement.remove()">
                &times;
            </button>
        </div>
    `;

    setTimeout(function () {
        contenedor.innerHTML = '';
    }, 4000);
}

function mostrarLoading(mostrar) {
    const loading = document.getElementById('loading');
    if (loading) {
        loading.style.display = mostrar ? 'block' : 'none';
    }
}

// ========================================
// PAGINACIÓN
// ========================================

function actualizarPaginacion(total) {
    const totalPaginas = Math.ceil(total / registrosPorPagina);
    const contenedor = document.getElementById('paginacionContainer');

    if (!contenedor) {
        return;
    }

    if (totalPaginas <= 1) {
        contenedor.style.display = 'none';
        return;
    }

    contenedor.style.display = 'block';

    const inicio = (paginaActual - 1) * registrosPorPagina + 1;
    const fin = Math.min(paginaActual * registrosPorPagina, total);

    asignarTexto('inicioMostrar', inicio);
    asignarTexto('finMostrar', fin);
    asignarTexto('totalRegistros', total);
    asignarTexto('numeroPagina', paginaActual);

    const btnAnterior = document.getElementById('btnAnterior');
    const btnSiguiente = document.getElementById('btnSiguiente');

    if (btnAnterior) {
        btnAnterior.disabled = paginaActual === 1;
    }
    if (btnSiguiente) {
        btnSiguiente.disabled = paginaActual === totalPaginas;
    }
}

function paginaAnterior() {
    if (paginaActual > 1) {
        paginaActual--;
        renderizarTipos();
    }
}

function paginaSiguiente() {
    const totalPaginas = Math.ceil(todosLosTipos.length / registrosPorPagina);
    if (paginaActual < totalPaginas) {
        paginaActual++;
        renderizarTipos();
    }
}

function renderizarTipos() {
    const inicio = (paginaActual - 1) * registrosPorPagina;
    const fin = inicio + registrosPorPagina;
    const tiposPagina = todosLosTipos.slice(inicio, fin);
    mostrarTipos(tiposPagina);
    actualizarPaginacion(todosLosTipos.length);
}

function mostrarTipos(tipos) {
    const container = document.getElementById('listaTiposUsuario');

    if (!container) {
        return;
    }

    if (!Array.isArray(tipos) || tipos.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-tags fa-4x"></i>
                <p>No hay tipos de usuario registrados</p>
                <button class="btn btn-primary" onclick="abrirModalCrear()">
                    <i class="bi bi-plus-circle"></i>
                    Crear primer tipo
                </button>
            </div>
        `;
        return;
    }

    let html = '';

    tipos.forEach(function (t) {
        html += `
            <div class="card-bordered">
                <div class="card-bordered-header">
                    <h3>
                        <i class="bi bi-tags"></i>
                        ${escaparHtml(t.nombre)}
                    </h3>
                    <span class="badge bg-secondary">ID: ${t.id}</span>
                </div>
                <div class="card-bordered-body">
                    <div class="info-row">
                        <span class="label">
                            <i class="bi bi-hash"></i>
                            ID:
                        </span>
                        <span class="value">${t.id}</span>
                    </div>
                </div>
                <div class="card-bordered-footer">
                    <button type="button" class="btn-sm btn-info" onclick="editarTipoUsuario(${t.id})">
                        <i class="bi bi-pencil"></i>
                        Editar
                    </button>
                    <button type="button" class="btn-sm btn-danger" onclick="abrirModalEliminar(${t.id}, '${escaparAtributo(t.nombre)}')">
                        <i class="bi bi-trash"></i>
                        Eliminar
                    </button>
                </div>
            </div>
        `;
    });

    container.innerHTML = html;
}

// ========================================
// CRUD - CARGAR TODOS
// ========================================

async function cargarTiposUsuario() {
    const headers = getHeaders();
    if (!headers) return;

    try {
        mostrarLoading(true);
        const response = await fetch(API_URL, { headers });

        if (handleUnauthorized(response)) return;

        if (!response.ok) throw new Error('Error al cargar');
        const data = await response.json();
        todosLosTipos = data;
        paginaActual = 1;
        renderizarTipos();
    } catch (error) {
        document.getElementById('listaTiposUsuario').innerHTML = `
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle"></i>
                Error al cargar tipos de usuario: ${error.message}
            </div>
        `;
    } finally {
        mostrarLoading(false);
    }
}

// ========================================
// MODALES
// ========================================

function abrirModalCrear() {
    document.getElementById('tituloModal').textContent = 'Nuevo Tipo de Usuario';
    document.getElementById('idTipoUsuario').value = '0';
    document.getElementById('nombre').value = '';
    limpiarErrores();
    document.getElementById('modalFormulario').style.display = 'block';
}

function cerrarModalFormulario() {
    document.getElementById('modalFormulario').style.display = 'none';
    limpiarErrores();
}

function abrirModalBuscar() {
    document.getElementById('buscarId').value = '';
    const resultado = document.getElementById('buscarResultado');
    resultado.style.display = 'none';
    resultado.innerHTML = '';
    document.getElementById('modalBuscar').style.display = 'block';
}

function cerrarModalBuscar() {
    document.getElementById('modalBuscar').style.display = 'none';
}

function abrirModalEliminar(id, nombre) {
    idEliminar = id;
    document.getElementById('nombreEliminar').textContent = nombre;
    document.getElementById('modalEliminar').style.display = 'block';
}

function cerrarModalEliminar() {
    document.getElementById('modalEliminar').style.display = 'none';
}

// ========================================
// BUSCAR POR ID
// ========================================

async function buscarTipoUsuarioPorId() {
    const id = document.getElementById('buscarId').value.trim();

    if (!id || parseInt(id) <= 0) {
        mostrarAlerta('Ingrese un ID válido mayor a 0', 'warning');
        return;
    }

    const headers = getHeaders();
    if (!headers) return;

    const resultado = document.getElementById('buscarResultado');
    resultado.style.display = 'block';
    resultado.innerHTML = `
        <div class="loading">
            <div class="spinner-border"></div>
            <p>Buscando...</p>
        </div>
    `;

    try {
        const response = await fetch(`${API_URL}/${id}`, { headers });

        if (handleUnauthorized(response)) return;

        if (response.status === 404) {
            resultado.innerHTML = `
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle"></i>
                    No se encontró ningún tipo de usuario con ID ${id}
                </div>
            `;
            return;
        }

        if (!response.ok) throw new Error('Error al buscar');
        const data = await response.json();

        resultado.innerHTML = `
            <div style="background: #f8f9fa; padding: 15px; border-radius: 8px; border: 1px solid #dee2e6;">
                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 8px; font-size: 14px;">
                    <div><strong>ID:</strong> ${data.id}</div>
                    <div><strong>Nombre:</strong> ${data.nombre}</div>
                </div>
                <div style="margin-top: 10px; display: flex; gap: 8px;">
                    <button class="btn btn-warning btn-sm" onclick="editarTipoUsuario(${data.id})">Editar</button>
                    <button class="btn btn-danger btn-sm" onclick="abrirModalEliminar(${data.id}, '${data.nombre}')">Eliminar</button>
                    <button class="btn btn-secondary btn-sm" onclick="cerrarModalBuscar()">Cerrar</button>
                </div>
            </div>
        `;

    } catch (error) {
        resultado.innerHTML = `
            <div class="alert alert-danger">
                Error al buscar: ${error.message}
            </div>
        `;
    }
}

// ========================================
// VALIDACIONES
// ========================================

function limpiarErrores() {
    document.querySelectorAll('.is-invalid').forEach(function (el) {
        el.classList.remove('is-invalid');
    });
    document.querySelectorAll('.invalid-feedback').forEach(function (el) {
        el.textContent = '';
    });
}

function mostrarError(campo, mensaje) {
    const el = document.getElementById(campo);
    const err = document.getElementById('err' + campo.charAt(0).toUpperCase() + campo.slice(1));
    if (el) el.classList.add('is-invalid');
    if (err) err.textContent = mensaje;
}

function validarFormulario() {
    let valido = true;
    limpiarErrores();

    const nombre = document.getElementById('nombre').value.trim();

    if (!nombre) {
        mostrarError('nombre', 'El nombre es requerido');
        valido = false;
    } else if (nombre.length < 3 || nombre.length > 50) {
        mostrarError('nombre', 'El nombre debe tener entre 3 y 50 caracteres');
        valido = false;
    }

    return valido;
}

function obtenerDatosFormulario() {
    return {
        nombre: document.getElementById('nombre').value.trim()
    };
}

// ========================================
// CRUD - CREAR / EDITAR
// ========================================

async function guardarTipoUsuario() {
    if (!validarFormulario()) return;

    const headers = getHeaders();
    if (!headers) return;

    const id = document.getElementById('idTipoUsuario').value;
    const data = obtenerDatosFormulario();
    const isEdit = id !== '0';
    const url = isEdit ? `${API_URL}/${id}` : API_URL;
    const method = isEdit ? 'PUT' : 'POST';

    if (isEdit) {
        data.id = parseInt(id);
    }

    try {
        const response = await fetch(url, {
            method: method,
            headers: headers,
            body: JSON.stringify(data)
        });

        if (handleUnauthorized(response)) return;

        if (response.ok) {
            mostrarAlerta(isEdit ? 'Tipo de usuario actualizado' : 'Tipo de usuario creado', 'success');
            cerrarModalFormulario();
            cargarTiposUsuario();
        } else {
            const error = await response.json();
            mostrarAlerta(error.error || 'Error al guardar', 'danger');
        }
    } catch (error) {
        mostrarAlerta('Error de conexión: ' + error.message, 'danger');
    }
}

// ========================================
// CRUD - EDITAR (OBTENER POR ID)
// ========================================

async function editarTipoUsuario(id) {
    const headers = getHeaders();
    if (!headers) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, { headers });

        if (handleUnauthorized(response)) return;

        if (!response.ok) throw new Error('Error al obtener datos');
        const data = await response.json();

        document.getElementById('tituloModal').textContent = 'Editar Tipo de Usuario';
        document.getElementById('idTipoUsuario').value = data.id;
        document.getElementById('nombre').value = data.nombre;
        limpiarErrores();
        document.getElementById('modalFormulario').style.display = 'block';
    } catch (error) {
        mostrarAlerta('Error al cargar datos: ' + error.message, 'danger');
    }
}

// ========================================
// CRUD - ELIMINAR
// ========================================

async function eliminarTipoUsuario() {
    const headers = getHeaders();
    if (!headers) return;

    try {
        const response = await fetch(`${API_URL}/${idEliminar}`, {
            method: 'DELETE',
            headers: headers
        });

        if (handleUnauthorized(response)) return;

        if (response.ok) {
            cerrarModalEliminar();
            mostrarAlerta('Tipo de usuario eliminado correctamente', 'success');
            cargarTiposUsuario();
        } else {
            const error = await response.json();
            mostrarAlerta(error.error || 'Error al eliminar', 'danger');
        }
    } catch (error) {
        mostrarAlerta('Error de conexión: ' + error.message, 'danger');
    }
}

// ========================================
// UTILIDADES
// ========================================

function escaparHtml(valor) {
    return String(valor || '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}

function escaparAtributo(valor) {
    return escaparHtml(valor)
        .replaceAll('\\', '\\\\')
        .replaceAll("'", "\\'")
        .replaceAll('\r', '')
        .replaceAll('\n', ' ');
}

function asignarTexto(id, valor) {
    const elemento = document.getElementById(id);
    if (elemento) {
        elemento.textContent = String(valor);
    }
}