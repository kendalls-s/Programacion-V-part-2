package com.example.movil

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.widget.Button
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.example.movil.network.ApiClient
import kotlinx.coroutines.*

class MainActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        val prefs = getSharedPreferences("AppPrefs", Context.MODE_PRIVATE)
        val token = prefs.getString("token", null)
        val userId = prefs.getString("userId", null)

        // Si no hay token o userId, redirigir al Login
        if (token == null || userId == null) {
            val intent = Intent(this, LoginActivity::class.java)
            startActivity(intent)
            finish()
            return
        }

        ApiClient.token = token
        cargarUsuario(userId)

        // 🔥 BOTÓN DE CERRAR SESIÓN
        val btnCerrarSesion = findViewById<Button>(R.id.btnCerrarSesion)
        btnCerrarSesion.setOnClickListener {
            // Limpiar SharedPreferences
            val prefs = getSharedPreferences("AppPrefs", Context.MODE_PRIVATE)
            prefs.edit().clear().apply()

            // Limpiar el token en ApiClient
            ApiClient.token = null

            // Redirigir al Login
            val intent = Intent(this, LoginActivity::class.java)
            intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
            startActivity(intent)
            finish()
        }
    }

    private fun cargarUsuario(id: String) {
        val tvNombre = findViewById<TextView>(R.id.tvNombre)
        val tvEmail = findViewById<TextView>(R.id.tvEmail)
        val tvCarrera = findViewById<TextView>(R.id.tvCarrera)

        CoroutineScope(Dispatchers.IO).launch {
            try {
                val usuario = ApiClient.usuarioApi.obtenerUsuario(id)
                withContext(Dispatchers.Main) {
                    tvNombre.text = usuario.nombreCompleto
                    tvEmail.text = usuario.email
                    tvCarrera.text = usuario.carreraOArea()
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    Toast.makeText(this@MainActivity, "Error: ${e.message}", Toast.LENGTH_LONG).show()
                }
            }
        }
    }
}