package com.example.movil.model

import com.google.gson.annotations.SerializedName

data class Usuario(
    @SerializedName("id") val id: Int,
    @SerializedName("email") val email: String,
    @SerializedName("numeroIdentificacion") val numeroIdentificacion: String,
    @SerializedName("nombreCompleto") val nombreCompleto: String,
    @SerializedName("tipoUsuario") val tipoUsuario: String,
    @SerializedName("carreras") val carreras: List<Carrera>? = null,
    @SerializedName("areas") val areas: List<Area>? = null
) {
    fun carreraOArea(): String =
        carreras?.firstOrNull()?.nombre
            ?: areas?.firstOrNull()?.nombre
            ?: "-"
}

data class Carrera(
    @SerializedName("nombre") val nombre: String
)

data class Area(
    @SerializedName("nombre") val nombre: String
)