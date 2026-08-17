package com.example.movil.network

import com.example.movil.model.Usuario
import okhttp3.ResponseBody
import retrofit2.http.GET
import retrofit2.http.Path
import retrofit2.http.Query

interface UsuarioApiService {

    @GET("gateway/usuarios/{id}")
    suspend fun obtenerUsuario(
        @Path("id") id: String
    ): Usuario

    @GET("gateway/fotografia/{id}")
    suspend fun obtenerFotografia(
        @Path("id") id: String
    ): ResponseBody

    @GET("gateway/qr")
    suspend fun obtenerQr(
        @Query("identificacion") identificacion: String
    ): ResponseBody
}