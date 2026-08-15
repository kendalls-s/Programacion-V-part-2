package com.example.movil.network

import com.example.movil.model.Usuario
import retrofit2.http.GET
import retrofit2.http.Path

interface UsuarioApiService {
    @GET("gateway/usuarios/{id}")
    suspend fun obtenerUsuario(@Path("id") id: String): Usuario
}