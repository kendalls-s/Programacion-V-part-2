// ========================================
// ROLES - JAVASCRIPT
// ========================================

let idEliminar = 0;
let paginaActual = 1;

const registrosPorPagina = 15;

// ========================================
// DATOS INICIALES
// ========================================

let todosLosRoles = normalizarListaRoles(
    window.rolesIniciales
);

const pantallasDisponibles =
    normalizarListaPantallas(
        window.pantallasDisponibles
    );

// Temporal para revisar la estructura recibida
console.log(
    'window.rolesIniciales:',
    window.rolesIniciales
);

console.log(
    'todosLosRoles normalizados:',
    todosLosRoles
);

// ========================================
// NORMALIZACIÓN DE DATOS
// ========================================

function normalizarListaRoles(respuesta) {
    if (!respuesta) {
        return [];
    }

    let lista = [];

    if (Array.isArray(respuesta)) {
        lista = respuesta;
    }
    else if (Array.isArray(respuesta.data)) {
        lista = respuesta.data;
    }
    else if (Array.isArray(respuesta.Data)) {
        lista = respuesta.Data;
    }
    else if (Array.isArray(respuesta.roles)) {
        lista = respuesta.roles;
    }
    else if (Array.isArray(respuesta.Roles)) {
        lista = respuesta.Roles;
    }

    return lista.map(normalizarRol);
}

function normalizarRol(rol) {
    if (!rol || typeof rol !== 'object') {
        return {
            id: null,
            nombre: 'Sin nombre',
            pantallas: []
        };
    }

    const id =
        rol.id ??
        rol.Id ??
        rol.ID ??
        rol.rolId ??
        rol.RolId ??
        rol.ROL_ID ??
        null;

    const nombre =
        rol.nombre ??
        rol.Nombre ??
        rol.NOMBRE ??
        rol.nombreRol ??
        rol.NombreRol ??
        'Sin nombre';

    const pantallasOriginales =
        rol.pantallas ??
        rol.Pantallas ??
        rol.PANTALLAS ??
        rol.listaPantallas ??
        rol.ListaPantallas ??
        [];

    return {
        id: id !== null && id !== undefined
            ? Number(id)
            : null,

        nombre: String(nombre),

        pantallas:
            normalizarListaPantallas(
                pantallasOriginales
            )
    };
}

function normalizarListaPantallas(respuesta) {
    if (!respuesta) {
        return [];
    }

    let lista = [];

    if (Array.isArray(respuesta)) {
        lista = respuesta;
    }
    else if (Array.isArray(respuesta.data)) {
        lista = respuesta.data;
    }
    else if (Array.isArray(respuesta.Data)) {
        lista = respuesta.Data;
    }
    else if (
        typeof respuesta === 'string'
    ) {
        lista = respuesta
            .split(',')
            .map(function (valor) {
                return valor.trim();
            })
            .filter(function (valor) {
                return valor !== '';
            });
    }

    return lista.map(function (pantalla) {
        if (
            typeof pantalla === 'number'
        ) {
            return {
                id: pantalla,
                nombre: `Pantalla ${pantalla}`
            };
        }

        if (
            typeof pantalla === 'string'
        ) {
            const posibleId =
                Number(pantalla);

            return {
                id: Number.isNaN(posibleId)
                    ? null
                    : posibleId,

                nombre: Number.isNaN(posibleId)
                    ? pantalla
                    : `Pantalla ${posibleId}`
            };
        }

        if (
            pantalla &&
            typeof pantalla === 'object'
        ) {
            const id =
                pantalla.id ??
                pantalla.Id ??
                pantalla.ID ??
                pantalla.pantallaId ??
                pantalla.PantallaId ??
                pantalla.PANTALLA_ID ??
                null;

            const nombre =
                pantalla.nombre ??
                pantalla.Nombre ??
                pantalla.NOMBRE ??
                pantalla.descripcion ??
                pantalla.Descripcion ??
                pantalla.ruta ??
                pantalla.Ruta ??
                '';

            return {
                id: id !== null &&
                    id !== undefined
                    ? Number(id)
                    : null,

                nombre: String(nombre)
            };
        }

        return {
            id: null,
            nombre: ''
        };
    });
}

// ========================================
// ANTIFORGERY
// ========================================

function obtenerTokenAntiforgery() {
    const input = document.querySelector(
        '#formAntiforgery input[name="__RequestVerificationToken"]'
    );

    return input ? input.value : '';
}

function getHeaders() {
    return {
        'Content-Type': 'application/json',
        'RequestVerificationToken':
            obtenerTokenAntiforgery()
    };
}

// ========================================
// MODALES
// ========================================

function abrirModalCrear() {
    document.getElementById(
        'tituloModal'
    ).textContent = 'Nuevo Rol';

    document.getElementById(
        'idRol'
    ).value = '0';

    document.getElementById(
        'nombre'
    ).value = '';

    document
        .querySelectorAll(
            '.pantalla-checkbox'
        )
        .forEach(function (checkbox) {
            checkbox.checked = false;
        });

    limpiarErrores();

    document
        .getElementById(
            'modalFormulario'
        )
        .classList
        .add('show');
}

function cerrarModalFormulario() {
    document
        .getElementById(
            'modalFormulario'
        )
        .classList
        .remove('show');

    limpiarErrores();
}

function abrirModalBuscar() {
    document.getElementById(
        'buscarId'
    ).value = '';

    const resultado =
        document.getElementById(
            'buscarResultado'
        );

    resultado.style.display = 'none';
    resultado.innerHTML = '';

    document
        .getElementById(
            'modalBuscar'
        )
        .classList
        .add('show');
}

function cerrarModalBuscar() {
    document
        .getElementById(
            'modalBuscar'
        )
        .classList
        .remove('show');
}

function abrirModalEliminar(
    id,
    nombre
) {
    idEliminar = Number(id);

    document.getElementById(
        'nombreEliminar'
    ).textContent = nombre;

    document
        .getElementById(
            'modalEliminar'
        )
        .classList
        .add('show');
}

function cerrarModalEliminar() {
    document
        .getElementById(
            'modalEliminar'
        )
        .classList
        .remove('show');
}

// ========================================
// ALERTAS
// ========================================

function mostrarAlerta(
    mensaje,
    tipo
) {
    const contenedor =
        document.getElementById(
            'alertas'
        );

    if (!contenedor) {
        console.error(mensaje);
        return;
    }

    contenedor.innerHTML = `
        <div class="alert alert-${tipo} alert-dismissible">
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
    }, 4000);
}

function mostrarLoading(mostrar) {
    const loading =
        document.getElementById(
            'loading'
        );

    if (loading) {
        loading.style.display =
            mostrar
                ? 'block'
                : 'none';
    }
}

// ========================================
// PAGINACIÓN
// ========================================

function actualizarPaginacion(total) {
    const totalPaginas =
        Math.ceil(
            total / registrosPorPagina
        );

    const contenedor =
        document.getElementById(
            'paginacionContainer'
        );

    if (!contenedor) {
        return;
    }

    if (totalPaginas <= 1) {
        contenedor.style.display =
            'none';

        return;
    }

    contenedor.style.display =
        'block';

    const inicio =
        (paginaActual - 1) *
        registrosPorPagina + 1;

    const fin =
        Math.min(
            paginaActual *
            registrosPorPagina,
            total
        );

    asignarTexto(
        'inicioMostrar',
        inicio
    );

    asignarTexto(
        'finMostrar',
        fin
    );

    asignarTexto(
        'totalRegistros',
        total
    );

    asignarTexto(
        'numeroPagina',
        paginaActual
    );

    const btnAnterior =
        document.getElementById(
            'btnAnterior'
        );

    const btnSiguiente =
        document.getElementById(
            'btnSiguiente'
        );

    if (btnAnterior) {
        btnAnterior.disabled =
            paginaActual === 1;
    }

    if (btnSiguiente) {
        btnSiguiente.disabled =
            paginaActual ===
            totalPaginas;
    }
}

function paginaAnterior() {
    if (paginaActual > 1) {
        paginaActual--;
        renderizarRoles();
    }
}

function paginaSiguiente() {
    const totalPaginas =
        Math.ceil(
            todosLosRoles.length /
            registrosPorPagina
        );

    if (
        paginaActual <
        totalPaginas
    ) {
        paginaActual++;
        renderizarRoles();
    }
}

function renderizarRoles() {
    const inicio =
        (paginaActual - 1) *
        registrosPorPagina;

    const fin =
        inicio +
        registrosPorPagina;

    const rolesPagina =
        todosLosRoles.slice(
            inicio,
            fin
        );

    mostrarRoles(rolesPagina);

    actualizarPaginacion(
        todosLosRoles.length
    );
}

// ========================================
// MOSTRAR ROLES
// ========================================

function mostrarRoles(roles) {
    const container =
        document.getElementById(
            'listaRoles'
        );

    if (!container) {
        return;
    }

    if (
        !Array.isArray(roles) ||
        roles.length === 0
    ) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="fas fa-user-tag fa-4x"></i>

                <p>
                    No hay roles registrados
                </p>

                <button class="btn btn-primary"
                        onclick="abrirModalCrear()">

                    <i class="fas fa-plus"></i>
                    Crear primer rol
                </button>
            </div>
        `;

        return;
    }

    let html = '';

    roles.forEach(function (
        rolOriginal
    ) {
        const rol =
            normalizarRol(
                rolOriginal
            );

        const id =
            rol.id;

        const nombre =
            rol.nombre ||
            'Sin nombre';

        const nombresPantallas =
            rol.pantallas
                .map(function (
                    pantalla
                ) {
                    return pantalla.nombre;
                })
                .filter(function (
                    nombrePantalla
                ) {
                    return String(
                        nombrePantalla
                    ).trim() !== '';
                });

        let pantallasHtml = '';

        if (
            nombresPantallas.length > 3
        ) {
            pantallasHtml =
                nombresPantallas
                    .slice(0, 3)
                    .map(function (
                        pantalla
                    ) {
                        return `
                            <span class="badge-items">
                                ${escaparHtml(pantalla)}
                            </span>
                        `;
                    })
                    .join(' ') +
                `
                    <span class="badge-items-more">
                        +${nombresPantallas.length - 3}
                    </span>
                `;
        }
        else {
            pantallasHtml =
                nombresPantallas
                    .map(function (
                        pantalla
                    ) {
                        return `
                            <span class="badge-items">
                                ${escaparHtml(pantalla)}
                            </span>
                        `;
                    })
                    .join(' ');
        }

        const idSeguro =
            id !== null &&
                id !== undefined &&
                !Number.isNaN(id)
                ? id
                : null;

        html += `
            <div class="card-bordered">

                <div class="card-bordered-header">

                    <h3>
                        <i class="fas fa-user-tag"></i>
                        ${escaparHtml(nombre)}
                    </h3>

                    <span class="rol-id">
                        ID:
                        ${idSeguro ??
            'No disponible'
            }
                    </span>

                </div>

                <div class="card-bordered-body">

                    <div class="info-row">

                        <span class="label">
                            <i class="fas fa-desktop"></i>
                            Pantallas:
                        </span>

                        <span class="value">
                            ${pantallasHtml ||
            `
                                <span class="text-muted">
                                    Ninguna
                                </span>
                                `
            }
                        </span>

                    </div>

                </div>

                <div class="card-bordered-footer">

                    <button
                        type="button"
                        class="btn-sm btn-info"
                        ${idSeguro === null
                ? 'disabled'
                : ''
            }
                        onclick="editarRol(${idSeguro})">

                        <i class="fas fa-edit"></i>
                        Editar
                    </button>

                    <button
                        type="button"
                        class="btn-sm btn-danger"
                        ${idSeguro === null
                ? 'disabled'
                : ''
            }
                        onclick="abrirModalEliminar(
                            ${idSeguro},
                            '${escaparAtributo(nombre)}'
                        )">

                        <i class="fas fa-trash"></i>
                        Eliminar
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

async function buscarRolPorId() {
    const id =
        document.getElementById(
            'buscarId'
        ).value.trim();

    if (
        !id ||
        Number(id) <= 0
    ) {
        mostrarAlerta(
            'Ingrese un ID válido mayor que cero.',
            'danger'
        );

        return;
    }

    const resultado =
        document.getElementById(
            'buscarResultado'
        );

    resultado.style.display =
        'block';

    resultado.innerHTML = `
        <div class="loading">
            <div class="spinner-border"></div>
            <p>Buscando...</p>
        </div>
    `;

    try {
        const response =
            await fetch(
                `/Roles?handler=Buscar&id=${encodeURIComponent(id)}`
            );

        const data =
            await leerRespuesta(
                response
            );

        if (
            !response.ok ||
            data.exito === false
        ) {
            resultado.innerHTML = `
                <div class="alert alert-danger">
                    ${escaparHtml(
                data.mensaje ||
                'No se encontró el rol.'
            )}
                </div>
            `;

            return;
        }

        const rol =
            normalizarRol(
                data.data ??
                data.Data ??
                data
            );

        if (
            rol.id === null ||
            rol.id === undefined
        ) {
            resultado.innerHTML = `
                <div class="alert alert-danger">
                    No se pudo interpretar la información del rol.
                </div>
            `;

            return;
        }

        const pantallasHtml =
            rol.pantallas.length > 0
                ? rol.pantallas
                    .map(function (
                        pantalla
                    ) {
                        return `
                            <span class="badge-items">
                                ${escaparHtml(
                            pantalla.nombre
                        )}
                            </span>
                        `;
                    })
                    .join(' ')
                : `
                    <span class="text-muted">
                        Ninguna
                    </span>
                `;

        resultado.innerHTML = `
            <div style="
                background: #f8f9fa;
                padding: 15px;
                border-radius: 8px;
                border: 1px solid #dee2e6;">

                <div>
                    <strong>ID:</strong>
                    ${rol.id}
                </div>

                <div style="margin-top: 6px;">
                    <strong>Nombre:</strong>
                    ${escaparHtml(
            rol.nombre
        )}
                </div>

                <div style="margin-top: 6px;">
                    <strong>Pantallas:</strong>

                    <div style="
                        margin-top: 6px;
                        display: flex;
                        gap: 6px;
                        flex-wrap: wrap;">

                        ${pantallasHtml}
                    </div>
                </div>

                <div style="
                    margin-top: 10px;
                    display: flex;
                    gap: 8px;
                    flex-wrap: wrap;">

                    <button
                        type="button"
                        class="btn btn-warning btn-sm"
                        onclick="editarRol(${rol.id})">

                        <i class="bi bi-pencil-square"></i>
                        Editar
                    </button>

                    <button
                        type="button"
                        class="btn btn-danger btn-sm"
                        onclick="abrirModalEliminar(
                            ${rol.id},
                            '${escaparAtributo(rol.nombre)}'
                        )">

                        <i class="bi bi-trash"></i>
                        Eliminar
                    </button>

                </div>
            </div>
        `;
    }
    catch (error) {
        resultado.innerHTML = `
            <div class="alert alert-danger">
                Error al buscar:
                ${escaparHtml(
            error.message
        )}
            </div>
        `;
    }
}

// ========================================
// VALIDACIONES
// ========================================

function limpiarErrores() {
    document
        .querySelectorAll(
            '.is-invalid'
        )
        .forEach(function (
            elemento
        ) {
            elemento.classList
                .remove(
                    'is-invalid'
                );
        });

    document
        .querySelectorAll(
            '.invalid-feedback'
        )
        .forEach(function (
            elemento
        ) {
            elemento.textContent = '';
        });
}

function mostrarError(
    campo,
    mensaje
) {
    const input =
        document.getElementById(
            campo
        );

    const error =
        document.getElementById(
            'err' +
            campo
                .charAt(0)
                .toUpperCase() +
            campo.slice(1)
        );

    if (input) {
        input.classList.add(
            'is-invalid'
        );
    }

    if (error) {
        error.textContent =
            mensaje;
    }
}

function obtenerPantallasSeleccionadas() {
    return Array.from(
        document.querySelectorAll(
            '.pantalla-checkbox:checked'
        )
    )
        .map(function (
            checkbox
        ) {
            return Number(
                checkbox.value
            );
        })
        .filter(function (
            id
        ) {
            return (
                !Number.isNaN(id) &&
                id > 0
            );
        });
}

function validarFormulario() {
    let valido = true;

    limpiarErrores();

    const nombre =
        document
            .getElementById(
                'nombre'
            )
            .value
            .trim();

    const pantallas =
        obtenerPantallasSeleccionadas();

    if (!nombre) {
        mostrarError(
            'nombre',
            'El nombre es requerido.'
        );

        valido = false;
    }

    if (
        nombre.length > 100
    ) {
        mostrarError(
            'nombre',
            'El nombre no puede superar los 100 caracteres.'
        );

        valido = false;
    }

    if (
        pantallas.length === 0
    ) {
        const errorPantallas =
            document.getElementById(
                'errPantallas'
            );

        if (errorPantallas) {
            errorPantallas.textContent =
                'Debe seleccionar al menos una pantalla.';
        }

        valido = false;
    }

    return valido;
}

function obtenerDatosFormulario() {
    return {
        nombre:
            document
                .getElementById(
                    'nombre'
                )
                .value
                .trim(),

        pantallas:
            obtenerPantallasSeleccionadas()
    };
}

// ========================================
// CREAR / EDITAR
// ========================================

async function guardarRol() {
    if (!validarFormulario()) {
        return;
    }

    const id =
        document.getElementById(
            'idRol'
        ).value;

    const esEdicion =
        id !== '0';

    const url =
        esEdicion
            ? `/Roles?handler=Editar&id=${encodeURIComponent(id)}`
            : '/Roles?handler=Crear';

    try {
        mostrarLoading(true);

        const response =
            await fetch(
                url,
                {
                    method: 'POST',
                    headers: getHeaders(),
                    body: JSON.stringify(
                        obtenerDatosFormulario()
                    )
                }
            );

        const data =
            await leerRespuesta(
                response
            );

        if (
            !response.ok ||
            data.exito === false
        ) {
            mostrarAlerta(
                data.mensaje ||
                'No fue posible guardar el rol.',
                'danger'
            );

            return;
        }

        mostrarAlerta(
            data.mensaje ||
            (
                esEdicion
                    ? 'Rol actualizado correctamente.'
                    : 'Rol creado correctamente.'
            ),
            'success'
        );

        cerrarModalFormulario();

        setTimeout(function () {
            window.location.reload();
        }, 700);
    }
    catch (error) {
        mostrarAlerta(
            'Error de conexión: ' +
            error.message,
            'danger'
        );
    }
    finally {
        mostrarLoading(false);
    }
}

// ========================================
// EDITAR
// ========================================

async function editarRol(id) {
    if (
        id === null ||
        id === undefined ||
        Number.isNaN(Number(id))
    ) {
        mostrarAlerta(
            'El ID del rol no es válido.',
            'danger'
        );

        return;
    }

    try {
        mostrarLoading(true);

        const response =
            await fetch(
                `/Roles?handler=Buscar&id=${encodeURIComponent(id)}`
            );

        const data =
            await leerRespuesta(
                response
            );

        if (
            !response.ok ||
            data.exito === false
        ) {
            mostrarAlerta(
                data.mensaje ||
                'No fue posible cargar el rol.',
                'danger'
            );

            return;
        }

        const rol =
            normalizarRol(
                data.data ??
                data.Data ??
                data
            );

        if (
            rol.id === null ||
            rol.id === undefined
        ) {
            mostrarAlerta(
                'La respuesta no contiene un ID de rol válido.',
                'danger'
            );

            return;
        }

        document.getElementById(
            'tituloModal'
        ).textContent =
            'Editar Rol';

        document.getElementById(
            'idRol'
        ).value =
            String(rol.id);

        document.getElementById(
            'nombre'
        ).value =
            rol.nombre;

        const idsPantallas =
            rol.pantallas
                .map(function (
                    pantalla
                ) {
                    return Number(
                        pantalla.id
                    );
                })
                .filter(function (
                    pantallaId
                ) {
                    return (
                        !Number.isNaN(
                            pantallaId
                        ) &&
                        pantallaId > 0
                    );
                });

        document
            .querySelectorAll(
                '.pantalla-checkbox'
            )
            .forEach(function (
                checkbox
            ) {
                checkbox.checked =
                    idsPantallas.includes(
                        Number(
                            checkbox.value
                        )
                    );
            });

        limpiarErrores();
        cerrarModalBuscar();

        document
            .getElementById(
                'modalFormulario'
            )
            .classList
            .add('show');
    }
    catch (error) {
        mostrarAlerta(
            'Error al cargar el rol: ' +
            error.message,
            'danger'
        );
    }
    finally {
        mostrarLoading(false);
    }
}

// ========================================
// ELIMINAR
// ========================================

async function eliminarRol() {
    if (
        !idEliminar ||
        Number(idEliminar) <= 0
    ) {
        mostrarAlerta(
            'El ID del rol no es válido.',
            'danger'
        );

        return;
    }

    try {
        mostrarLoading(true);

        const response =
            await fetch(
                `/Roles?handler=Eliminar&id=${encodeURIComponent(idEliminar)}`,
                {
                    method: 'POST',
                    headers: getHeaders()
                }
            );

        const data =
            await leerRespuesta(
                response
            );

        if (
            !response.ok ||
            data.exito === false
        ) {
            mostrarAlerta(
                data.mensaje ||
                'No fue posible eliminar el rol.',
                'danger'
            );

            return;
        }

        cerrarModalEliminar();

        mostrarAlerta(
            data.mensaje ||
            'Rol eliminado correctamente.',
            'success'
        );

        setTimeout(function () {
            window.location.reload();
        }, 700);
    }
    catch (error) {
        mostrarAlerta(
            'Error de conexión: ' +
            error.message,
            'danger'
        );
    }
    finally {
        mostrarLoading(false);
    }
}

// ========================================
// UTILIDADES
// ========================================

async function leerRespuesta(
    response
) {
    const texto =
        await response.text();

    if (!texto) {
        return {};
    }

    try {
        return JSON.parse(
            texto
        );
    }
    catch {
        return {
            exito: false,
            mensaje: texto
        };
    }
}

function asignarTexto(
    id,
    valor
) {
    const elemento =
        document.getElementById(
            id
        );

    if (elemento) {
        elemento.textContent =
            String(valor);
    }
}

function escaparHtml(valor) {
    return String(
        valor ?? ''
    )
        .replaceAll(
            '&',
            '&amp;'
        )
        .replaceAll(
            '<',
            '&lt;'
        )
        .replaceAll(
            '>',
            '&gt;'
        )
        .replaceAll(
            '"',
            '&quot;'
        )
        .replaceAll(
            "'",
            '&#039;'
        );
}

function escaparAtributo(valor) {
    return escaparHtml(valor)
        .replaceAll(
            '\\',
            '\\\\'
        )
        .replaceAll(
            "'",
            "\\'"
        )
        .replaceAll(
            '\r',
            ''
        )
        .replaceAll(
            '\n',
            ' '
        );
}

// ========================================
// INICIALIZAR
// ========================================

document.addEventListener(
    'DOMContentLoaded',
    function () {
        mostrarLoading(false);

        todosLosRoles =
            normalizarListaRoles(
                window.rolesIniciales
            );

        paginaActual = 1;

        renderizarRoles();
    }
);