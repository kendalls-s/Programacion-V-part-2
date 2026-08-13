package com.example.movil.model

import com.google.gson.annotations.SerializedName

/**
 * DTO de usuario para la pantalla USR2 (datos personales básicos).
 * Contrato real confirmado del servicio "Usuarios" del Gateway
 * (gateway/usuarios/{numeroIdentificacion}).
 */
data class Usuario(
    @SerializedName("id")
    val id: Int,

    @SerializedName("email")
    val email: String,

    @SerializedName("numeroIdentificacion")
    val numeroIdentificacion: String,

    @SerializedName("nombreCompleto")
    val nombreCompleto: String,

    @SerializedName("tipoUsuario")
    val tipoUsuario: String,

    // El usuario puede tener varias carreras o varias áreas según el tipo.
    @SerializedName("carreras")
    val carreras: List<Carrera>? = null,

    @SerializedName("areas")
    val areas: List<Area>? = null
) {
    /** Texto a mostrar como "carrera o área": se toma la primera que venga informada. */
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
