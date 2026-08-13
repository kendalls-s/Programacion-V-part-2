package com.example.movil.network

/**
 * Configuración general de acceso al API Gateway (Ocelot).
 *
 * TODO: cuando exista un servidor de despliegue fijo, mover BASE_URL a
 * BuildConfig / variables de entorno en lugar de dejarlo hardcodeado.
 */
object Constants {

    /**
     * Gateway hosteado en servidor real (dominio del CUC).
     */
    const val BASE_URL = "https://tiusr22pl.cuc-carrera-ti.ac.cr/Gateway/"

    /**
     * TODO: HU USR1 (login) aún no está implementada en esta app.
     * Mientras tanto se deja fijo el numeroIdentificacion de un usuario
     * de prueba para poder construir y probar la pantalla de USR2.
     */
    const val ID_USUARIO_HARDCODE = "45"
}
