package com.example.movil.model

import com.google.gson.annotations.SerializedName

data class CarnetQrData(
    @SerializedName("nombreCompleto")
    val nombreCompleto: String,

    @SerializedName("identificacion")
    val identificacion: String,

    @SerializedName("tipoUsuario")
    val tipoUsuario: String,

    @SerializedName("carrerasOAreas")
    val carrerasOAreas: List<String> = emptyList(),

    @SerializedName("institucion")
    val institucion: String,

    @SerializedName("fechaVencimiento")
    val fechaVencimiento: String
)