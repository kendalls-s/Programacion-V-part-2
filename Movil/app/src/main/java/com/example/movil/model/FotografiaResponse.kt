package com.example.movil.model

import com.google.gson.annotations.SerializedName

data class FotografiaResponse(
    @SerializedName("usuarioId") val usuarioId: Int? = null,
    @SerializedName("fotografiaBase64") val fotografiaBase64: String? = null
)
