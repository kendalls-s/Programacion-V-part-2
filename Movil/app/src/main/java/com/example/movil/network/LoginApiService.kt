package com.example.movil.network

import com.example.movil.model.LoginRequest
import com.example.movil.model.LoginResponse
import retrofit2.http.Body
import retrofit2.http.POST

interface LoginApiService {
    // 🔥 Esta es la ruta que debe funcionar
    @POST("gateway/login/login")
    suspend fun login(@Body request: LoginRequest): LoginResponse
}