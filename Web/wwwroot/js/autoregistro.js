document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formAutoRegistro");
    const mensaje = document.getElementById("mensajeRegistro");

    const tipoIdentificacion = document.getElementById("tipoIdentificacionId");
    const tipoUsuario = document.getElementById("tipoUsuarioId");
    const rol = document.getElementById("rolId");
    const instituciones = document.getElementById("instituciones");
    const carreras = document.getElementById("carrerasAsociadas");
    const areas = document.getElementById("areasAsociadas");

    const grupoCarreras = document.getElementById("grupoCarreras");
    const grupoAreas = document.getElementById("grupoAreas");

    const btnRegistrar = document.getElementById("btnRegistrar");
    const spinner = document.getElementById("spinnerRegistro");
    const iconoRegistro = document.getElementById("iconoRegistro");
    const textoRegistro = document.getElementById("textoRegistro");

    inicializar();

    async function inicializar() {
        configurarEventos();

        try {
            await Promise.all([
                cargarTiposIdentificacion(),
                cargarTiposUsuario(),
                cargarRoles(),
                cargarInstituciones()
            ]);
        } catch (error) {
            mostrarMensaje(
                error.message || "No se pudieron cargar los datos del formulario.",
                "error"
            );
        }
    }

    function configurarEventos() {
        tipoUsuario.addEventListener("change", actualizarCamposCondicionales);
        instituciones.addEventListener("change", cargarDatosInstitucionales);
        form.addEventListener("submit", registrarUsuario);

        document.querySelectorAll(".toggle-password").forEach(boton => {
            boton.addEventListener("click", () => {
                const input = document.getElementById(boton.dataset.target);
                const icono = boton.querySelector("i");

                if (!input || !icono) {
                    return;
                }

                const mostrar = input.type === "password";

                input.type = mostrar ? "text" : "password";

                icono.classList.toggle("fa-eye", !mostrar);
                icono.classList.toggle("fa-eye-slash", mostrar);

                boton.setAttribute(
                    "aria-label",
                    mostrar ? "Ocultar contraseña" : "Mostrar contraseña"
                );
            });
        });
    }

    async function cargarTiposIdentificacion() {
        const datos = await obtenerCatalogo(
            "/AutoRegistro?handler=TiposIdentificacion"
        );

        llenarSelect(
            tipoIdentificacion,
            datos,
            "Seleccione una opción"
        );
    }

    async function cargarTiposUsuario() {
        const datos = await obtenerCatalogo(
            "/AutoRegistro?handler=TiposUsuario"
        );

        llenarSelect(
            tipoUsuario,
            datos,
            "Seleccione una opción"
        );
    }

    async function cargarRoles() {
        const datos = await obtenerCatalogo(
            "/AutoRegistro?handler=Roles"
        );

        llenarSelect(
            rol,
            datos,
            "Seleccione una opción"
        );
    }

    async function cargarInstituciones() {
        const datos = await obtenerCatalogo(
            "/AutoRegistro?handler=Instituciones"
        );

        llenarSelect(
            instituciones,
            datos,
            "Seleccione una institución"
        );
    }

    async function cargarDatosInstitucionales() {
        const institucionId = Number(instituciones.value);

        limpiarSelect(carreras, "Seleccione una carrera");
        limpiarSelect(areas, "Seleccione un área");

        carreras.disabled = true;
        areas.disabled = true;

        if (!institucionId) {
            return;
        }

        try {
            const [datosCarreras, datosAreas] = await Promise.all([
                obtenerCatalogo(
                    `/AutoRegistro?handler=Carreras&institucionId=${institucionId}`
                ),
                obtenerCatalogo(
                    `/AutoRegistro?handler=Areas&institucionId=${institucionId}`
                )
            ]);

            llenarSelect(
                carreras,
                datosCarreras,
                "Seleccione una carrera"
            );

            llenarSelect(
                areas,
                datosAreas,
                "Seleccione un área"
            );

            carreras.disabled = false;
            areas.disabled = false;
        } catch (error) {
            mostrarMensaje(
                error.message ||
                "No se pudieron cargar las carreras y áreas.",
                "error"
            );
        }
    }

    function actualizarCamposCondicionales() {
        const opcionSeleccionada =
            tipoUsuario.options[tipoUsuario.selectedIndex];

        const nombreTipo =
            opcionSeleccionada?.textContent?.trim().toLowerCase() || "";

        const esEstudiante = nombreTipo.includes("estudiante");
        const esFuncionario =
            nombreTipo.includes("funcionario") ||
            nombreTipo.includes("administrador");

        grupoCarreras.style.display = esEstudiante ? "block" : "none";
        grupoAreas.style.display = esFuncionario ? "block" : "none";

        carreras.required = esEstudiante;
        areas.required = esFuncionario;

        if (!esEstudiante) {
            carreras.value = "";
        }

        if (!esFuncionario) {
            areas.value = "";
        }
    }

    async function registrarUsuario(event) {
        event.preventDefault();
        ocultarMensaje();

        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const contrasena =
            document.getElementById("contrasena").value;

        const confirmarContrasena =
            document.getElementById("confirmarContrasena").value;

        if (contrasena !== confirmarContrasena) {
            mostrarMensaje(
                "Las contraseñas no coinciden.",
                "error"
            );

            document.getElementById("confirmarContrasena").focus();
            return;
        }

        const telefono =
            document.getElementById("telefono").value.trim();

        if (!/^[0-9+\-\s]{8,20}$/.test(telefono)) {
            mostrarMensaje(
                "Ingrese un número de teléfono válido.",
                "error"
            );

            document.getElementById("telefono").focus();
            return;
        }

        const payload = {
            email: document.getElementById("email").value.trim(),
            contrasena,
            tipoUsuarioId: Number(tipoUsuario.value),
            tipoIdentificacionId: Number(tipoIdentificacion.value),
            numeroIdentificacion:
                document.getElementById("numeroIdentificacion").value.trim(),
            nombreCompleto:
                document.getElementById("nombreCompleto").value.trim(),
            rolId: Number(rol.value),
            instituciones: [Number(instituciones.value)],
            carrerasAsociadas: obtenerSeleccionOpcional(carreras),
            areasAsociadas: obtenerSeleccionOpcional(areas),
            telefonos: [telefono]
        };

        const token = obtenerTokenAntiforgery();

        cambiarEstadoBoton(true);

        try {
            const response = await fetch(
                "/AutoRegistro?handler=Registrar",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": token
                    },
                    body: JSON.stringify(payload)
                }
            );

            const resultado = await leerRespuesta(response);

            if (!response.ok || resultado.success === false) {
                throw new Error(
                    resultado.message ||
                    "No se pudo completar el registro."
                );
            }

            mostrarMensaje(
                resultado.message ||
                "Usuario registrado correctamente. Revise su correo para confirmar la cuenta.",
                "exito"
            );

            form.reset();

            limpiarSelect(carreras, "Seleccione una carrera");
            limpiarSelect(areas, "Seleccione un área");

            grupoCarreras.style.display = "none";
            grupoAreas.style.display = "none";

            carreras.required = false;
            areas.required = false;

            setTimeout(() => {
                window.location.href = "/Login";
            }, 3000);
        } catch (error) {
            mostrarMensaje(
                error.message ||
                "Ocurrió un error durante el registro.",
                "error"
            );
        } finally {
            cambiarEstadoBoton(false);
        }
    }

    async function obtenerCatalogo(url) {
        const response = await fetch(url, {
            method: "GET",
            headers: {
                "Accept": "application/json"
            }
        });

        const resultado = await leerRespuesta(response);

        if (!response.ok || resultado.success === false) {
            throw new Error(
                resultado.message ||
                "No se pudo cargar uno de los catálogos."
            );
        }

        return Array.isArray(resultado.data)
            ? resultado.data
            : [];
    }

    async function leerRespuesta(response) {
        const texto = await response.text();

        if (!texto) {
            return {};
        }

        try {
            return JSON.parse(texto);
        } catch {
            return {
                success: response.ok,
                message: texto
            };
        }
    }

    function llenarSelect(select, datos, textoInicial) {
        limpiarSelect(select, textoInicial);

        datos.forEach(item => {
            const option = document.createElement("option");

            option.value = item.id;
            option.textContent = item.nombre;

            select.appendChild(option);
        });
    }

    function limpiarSelect(select, textoInicial) {
        select.innerHTML = "";

        const option = document.createElement("option");
        option.value = "";
        option.textContent = textoInicial;

        select.appendChild(option);
    }

    function obtenerSeleccionOpcional(select) {
        const valor = Number(select.value);

        return valor > 0 ? [valor] : [];
    }

    function obtenerTokenAntiforgery() {
        const input = document.querySelector(
            'input[name="__RequestVerificationToken"]'
        );

        return input?.value || "";
    }

    function mostrarMensaje(texto, tipo) {
        mensaje.textContent = texto;
        mensaje.className = `mensaje ${tipo}`;
        mensaje.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });
    }

    function ocultarMensaje() {
        mensaje.textContent = "";
        mensaje.className = "mensaje";
    }

    function cambiarEstadoBoton(cargando) {
        btnRegistrar.disabled = cargando;

        spinner.style.display = cargando
            ? "inline-block"
            : "none";

        iconoRegistro.style.display = cargando
            ? "none"
            : "inline-block";

        textoRegistro.textContent = cargando
            ? "Creando cuenta..."
            : "Crear cuenta";
    }
});