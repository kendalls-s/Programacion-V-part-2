package com.example.movil.model

import com.google.gson.annotations.SerializedName

data class LoginResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("message") val message: String?,
    @SerializedName("accessToken") val accessToken: String?,
    @SerializedName("refreshToken") val refreshToken: String?,
    @SerializedName("tokenType") val tokenType: String?,
    @SerializedName("expiresIn") val expiresIn: Int?,
    @SerializedName("user") val user: UserData?
)

data class UserData(
    @SerializedName("id") val id: Int,
    @SerializedName("email") val email: String,
    @SerializedName("nombreCompleto") val nombreCompleto: String,
    @SerializedName("tipoUsuario") val tipoUsuario: String
)