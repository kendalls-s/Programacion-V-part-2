package com.example.movil

import android.graphics.BitmapFactory
import android.os.Bundle
import android.util.Base64
import android.view.View
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.lifecycle.lifecycleScope
import com.example.movil.databinding.ActivityMainBinding
import com.example.movil.model.Usuario
import com.example.movil.network.ApiClient
import com.example.movil.network.Constants
import com.google.gson.Gson
import kotlinx.coroutines.launch

/**
 * HU USR2: pantalla de datos personales básicos del usuario autenticado.
 */
class MainActivity : AppCompatActivity() {

    private lateinit var binding: ActivityMainBinding

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()

        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        ViewCompat.setOnApplyWindowInsetsListener(binding.main) { v, insets ->
            val systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars())
            v.setPadding(
                systemBars.left,
                systemBars.top,
                systemBars.right,
                systemBars.bottom
            )
            insets
        }

        cargarUsuario(Constants.ID_USUARIO_HARDCODE)
    }

    private fun cargarUsuario(id: String) {
        mostrarCargando(true)

        lifecycleScope.launch {
            try {
                val usuario = ApiClient.usuarioApi.obtenerUsuario(id)

                mostrarCargando(false)
                bindUsuario(usuario)

            } catch (e: Exception) {
                e.printStackTrace()
                mostrarCargando(false)
                mostrarError()
            }
        }
    }

    private fun bindUsuario(usuario: Usuario) {
        binding.tvNombreCompleto.text = usuario.nombreCompleto
        binding.tvIdentificacion.text = usuario.numeroIdentificacion
        binding.tvTipoUsuario.text = usuario.tipoUsuario
        binding.tvCarreraArea.text = usuario.carreraOArea()

        // El endpoint de fotografía utiliza el ID interno.
        cargarFotografia(usuario.id.toString())
    }

    /**
     * Obtiene la fotografía como Base64, la decodifica y la muestra.
     *
     * El endpoint devuelve:
     *
     * "/9j/4AAQSkZJRgABAQEASABIAAD/..."
     *
     * y NO image/jpeg directamente.
     */
    private fun cargarFotografia(idUsuario: String) {
        lifecycleScope.launch {
            try {
                val response =
                    ApiClient.usuarioApi.obtenerFotografia(idUsuario)

                val body = response.string().trim()

                if (body.isBlank()) {
                    mostrarFotoNoDisponible()
                    return@launch
                }

                // Si viene como JSON String:
                // "/9j/4AAQSkZJR..."
                // Gson elimina las comillas.
                val base64 = if (
                    body.startsWith("\"") &&
                    body.endsWith("\"")
                ) {
                    Gson().fromJson(body, String::class.java)
                } else {
                    body
                }
                    .trim()
                    .replaceFirst(
                        Regex("^data:image/[^;]+;base64,"),
                        ""
                    )
                    .replace("\n", "")
                    .replace("\r", "")

                if (base64.isBlank()) {
                    mostrarFotoNoDisponible()
                    return@launch
                }

                val imageBytes = Base64.decode(
                    base64,
                    Base64.DEFAULT
                )

                val bitmap = BitmapFactory.decodeByteArray(
                    imageBytes,
                    0,
                    imageBytes.size
                )

                if (bitmap != null) {
                    binding.ivFoto.setImageBitmap(bitmap)
                    binding.tvLeyendaSinFoto.visibility = View.GONE
                } else {
                    mostrarFotoNoDisponible()
                }

            } catch (e: Exception) {
                e.printStackTrace()
                mostrarFotoNoDisponible()
            }
        }
    }

    private fun mostrarFotoNoDisponible() {
        binding.ivFoto.setImageResource(
            R.drawable.ic_avatar_placeholder
        )

        binding.tvLeyendaSinFoto.visibility = View.VISIBLE
    }

    private fun mostrarCargando(cargando: Boolean) {
        binding.progressBar.visibility =
            if (cargando) View.VISIBLE else View.GONE

        binding.tvError.visibility = View.GONE
    }

    private fun mostrarError() {
        binding.tvError.text =
            getString(R.string.error_carga_usuario)

        binding.tvError.visibility = View.VISIBLE
    }
}