package com.example.movil.network

import com.example.movil.model.Usuario
import okhttp3.ResponseBody
import retrofit2.http.GET
import retrofit2.http.Path

/**
 * Rutas del Gateway (Ocelot) que se consumen desde la app móvil.
 */
interface UsuarioApiService {

    // GET /Gateway/gateway/usuarios/{id}
    @GET("gateway/usuarios/{id}")
    suspend fun obtenerUsuario(@Path("id") id: String): Usuario

    @GET("gateway/fotografia/{id}")
    suspend fun obtenerFotografia(@Path("id") id: String): ResponseBody
}