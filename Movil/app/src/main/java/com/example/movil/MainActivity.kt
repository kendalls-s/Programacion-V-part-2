package com.example.movil

import android.content.Context
import android.content.Intent
import android.graphics.BitmapFactory
import android.os.Bundle
import android.util.Base64
import android.view.View
import android.widget.Button
import android.widget.ImageButton
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.lifecycle.lifecycleScope
import androidx.viewpager2.widget.ViewPager2
import com.example.movil.model.FotografiaResponse
import com.example.movil.model.Usuario
import com.example.movil.network.ApiClient
import com.example.movil.ui.MainPagerAdapter
import com.google.gson.Gson
import kotlinx.coroutines.launch

class MainActivity : AppCompatActivity() {

    private lateinit var viewPager: ViewPager2
    private lateinit var progressBar: ProgressBar
    private lateinit var tvError: TextView

    private lateinit var ivFoto: ImageView
    private lateinit var tvLeyendaSinFoto: TextView
    private lateinit var tvNombreCompleto: TextView
    private lateinit var tvTipoUsuario: TextView
    private lateinit var tvIdentificacion: TextView
    private lateinit var groupCarreraArea: LinearLayout
    private lateinit var tvCarreraArea: TextView
    private lateinit var btnCerrarSesion: Button
    private lateinit var btnCamara: ImageButton

    private var esGuarda: Boolean = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val prefs = getSharedPreferences("AppPrefs", Context.MODE_PRIVATE)
        val token = prefs.getString("token", null)
        val userId = prefs.getString("userId", null)

        if (token == null || userId == null) {
            startActivity(Intent(this, LoginActivity::class.java))
            finish()
            return
        }

        ApiClient.token = token

        viewPager = findViewById(R.id.viewPager)
        progressBar = findViewById(R.id.progressBar)
        tvError = findViewById(R.id.tvError)

        cargarUsuario(userId)
    }

    private fun cargarUsuario(id: String) {
        mostrarCargando(true)

        lifecycleScope.launch {
            try {
                val usuario = ApiClient.usuarioApi.obtenerUsuario(id)
                mostrarCargando(false)
                configurarPager(usuario, id)
            } catch (e: Exception) {
                e.printStackTrace()
                mostrarCargando(false)
                mostrarError()
            }
        }
    }

    private fun configurarPager(usuario: Usuario, idUsuario: String) {
        esGuarda = usuario.tipoUsuario.equals("Guarda", ignoreCase = true)
        val tituloQr = if (esGuarda) getString(R.string.label_scan_qr) else getString(R.string.label_foto_qr)

        viewPager.isUserInputEnabled = false

        viewPager.adapter = MainPagerAdapter(tituloQr) { vistaDatos ->
            bindDatosView(vistaDatos, usuario, esGuarda)
            cargarFotografia(idUsuario, vistaDatos)
        }
    }

    private fun bindDatosView(root: View, usuario: Usuario, esGuarda: Boolean) {
        ivFoto = root.findViewById(R.id.ivFoto)
        tvLeyendaSinFoto = root.findViewById(R.id.tvLeyendaSinFoto)
        tvNombreCompleto = root.findViewById(R.id.tvNombreCompleto)
        tvTipoUsuario = root.findViewById(R.id.tvTipoUsuario)
        tvIdentificacion = root.findViewById(R.id.tvIdentificacion)
        groupCarreraArea = root.findViewById(R.id.groupCarreraArea)
        tvCarreraArea = root.findViewById(R.id.tvCarreraArea)
        btnCerrarSesion = root.findViewById(R.id.btnCerrarSesion)
        btnCamara = root.findViewById(R.id.btnCamara)

        tvNombreCompleto.text = usuario.nombreCompleto
        tvIdentificacion.text = usuario.numeroIdentificacion
        tvTipoUsuario.text = usuario.tipoUsuario

        if (esGuarda) {
            groupCarreraArea.visibility = View.GONE
            btnCamara.visibility = View.VISIBLE
            btnCamara.isEnabled = false
            btnCamara.setOnClickListener {
                viewPager.currentItem = 1
            }
        } else {
            groupCarreraArea.visibility = View.VISIBLE
            tvCarreraArea.text = usuario.carreraOArea()
            btnCamara.visibility = View.GONE
        }

        btnCerrarSesion.setOnClickListener {
            val prefs = getSharedPreferences("AppPrefs", Context.MODE_PRIVATE)
            prefs.edit().clear().apply()
            ApiClient.token = null
            val intent = Intent(this, LoginActivity::class.java)
            intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            startActivity(intent)
            finish()
        }
    }

    private fun cargarFotografia(idUsuario: String, root: View) {
        lifecycleScope.launch {
            try {
                val response = ApiClient.usuarioApi.obtenerFotografia(idUsuario)
                val body = response.string().trim()

                if (body.isBlank()) {
                    mostrarFotoNoDisponible()
                    return@launch
                }

                val crudo = when {
                    body.startsWith("{") -> {
                        val obj = Gson().fromJson(body, FotografiaResponse::class.java)
                        obj?.fotografiaBase64
                    }
                    body.startsWith("\"") && body.endsWith("\"") -> {
                        Gson().fromJson(body, String::class.java)
                    }
                    else -> body
                }

                val base64 = crudo
                    ?.trim()
                    ?.replaceFirst(Regex("^data:image/[^;]+;base64,"), "")
                    ?.replace("\n", "")
                    ?.replace("\r", "")
                    ?: ""

                if (base64.isBlank()) {
                    mostrarFotoNoDisponible()
                    return@launch
                }

                val imageBytes = Base64.decode(base64, Base64.DEFAULT)
                val bitmap = BitmapFactory.decodeByteArray(imageBytes, 0, imageBytes.size)

                if (bitmap != null) {
                    ivFoto.setImageBitmap(bitmap)
                    tvLeyendaSinFoto.visibility = View.GONE
                    if (esGuarda) {
                        btnCamara.isEnabled = true
                    } else {
                        viewPager.isUserInputEnabled = true
                    }
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
        ivFoto.setImageResource(R.drawable.ic_avatar_placeholder)
        tvLeyendaSinFoto.text = getString(R.string.leyenda_sin_foto)
        tvLeyendaSinFoto.visibility = View.VISIBLE
        if (esGuarda) {
            btnCamara.isEnabled = false
        } else {
            viewPager.isUserInputEnabled = false
        }
    }

    private fun mostrarCargando(cargando: Boolean) {
        progressBar.visibility = if (cargando) View.VISIBLE else View.GONE
        tvError.visibility = View.GONE
    }

    private fun mostrarError() {
        tvError.text = getString(R.string.error_carga_usuario)
        tvError.visibility = View.VISIBLE
    }
}
