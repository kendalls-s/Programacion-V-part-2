package com.example.movil.network

import com.example.movil.model.Usuario
import okhttp3.ResponseBody
import retrofit2.http.GET
import retrofit2.http.Path

/**
 * Rutas del Gateway (Ocelot) que se consumen desde la app móvil.
 * Ambos endpoints van por el cliente autenticado (usuarioApi en ApiClient),
 * que agrega automáticamente el header Authorization: Bearer <token>.
 */
interface UsuarioApiService {

    // GET /Gateway/gateway/usuarios/{id}
    @GET("gateway/usuarios/{id}")
    suspend fun obtenerUsuario(@Path("id") id: String): Usuario

    /**
     * GET /Gateway/gateway/fotografia/{id}
     *
     * El endpoint devuelve una cadena Base64 dentro de JSON, por ejemplo:
     * "/9j/4AAQSkZJRgABAQEASABIAAD/..."
     *
     * Se recibe como ResponseBody para poder manejar tanto la respuesta
     * JSON entre comillas como una respuesta de texto plano.
     */
    @GET("gateway/fotografia/{id}")
    suspend fun obtenerFotografia(@Path("id") id: String): ResponseBody
}
