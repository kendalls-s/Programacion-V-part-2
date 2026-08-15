package com.example.movil

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.example.movil.model.LoginRequest
import com.example.movil.network.ApiClient
import kotlinx.coroutines.*

class LoginActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_login)

        val etEmail = findViewById<EditText>(R.id.etEmail)
        val etPassword = findViewById<EditText>(R.id.etPassword)
        val spTipoUsuario = findViewById<Spinner>(R.id.spTipoUsuario)
        val btnLogin = findViewById<Button>(R.id.btnLogin)

        // 🔥 LISTA COMPLETA DE TIPOS SEGÚN LA BASE DE DATOS
        val tipos = arrayOf(
            "Seleccione tipo",
            "Funcionario",
            "Estudiante",
            "Administrativo",
            "Conserje",
            "Guarda"
        )
        val adapter = ArrayAdapter(this, android.R.layout.simple_spinner_item, tipos)
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        spTipoUsuario.adapter = adapter

        btnLogin.setOnClickListener {
            val email = etEmail.text.toString().trim()
            val password = etPassword.text.toString().trim()
            val tipoSeleccionado = spTipoUsuario.selectedItem.toString()

            if (tipoSeleccionado == "Seleccione tipo") {
                Toast.makeText(this, "Seleccione un tipo de usuario", Toast.LENGTH_SHORT).show()
                return@setOnClickListener
            }

            // 🔥 CONVERTIR EL TIPO (usando los nombres exactos de la BD)
            val tipo = when (tipoSeleccionado) {
                "Funcionario" -> "Funcionario"
                "Estudiante" -> "Estudiante"
                "Administrativo" -> "Administrativo"
                "Conserje" -> "Conserje"
                "Guarda" -> "Guarda"
                else -> ""
            }

            if (email.isNotBlank() && password.isNotBlank() && tipo != "") {
                realizarLogin(email, password, tipo)
            } else {
                Toast.makeText(this, "Complete todos los campos", Toast.LENGTH_SHORT).show()
            }
        }
    }

    private fun realizarLogin(email: String, password: String, tipo: String) {
        val request = LoginRequest(email.trim(), password, tipo)

        CoroutineScope(Dispatchers.IO).launch {
            try {
                val response = ApiClient.loginApi.login(request)

                // 🔒 VALIDACIÓN: Comparar el tipo seleccionado con el del backend
                val tipoSeleccionado = tipo.trim()
                val tipoBackend = response.user?.tipoUsuario?.trim() ?: ""

                if (tipoSeleccionado != tipoBackend) {
                    withContext(Dispatchers.Main) {
                        Toast.makeText(
                            this@LoginActivity,
                            "Tipo de usuario incorrecto. El usuario es: $tipoBackend",
                            Toast.LENGTH_LONG
                        ).show()
                    }
                    return@launch
                }

                if (response.success == true && response.accessToken != null) {
                    val prefs = getSharedPreferences("AppPrefs", Context.MODE_PRIVATE)
                    prefs.edit().putString("token", response.accessToken).apply()
                    prefs.edit().putString("userId", response.user?.id?.toString() ?: "").apply()

                    ApiClient.token = response.accessToken

                    withContext(Dispatchers.Main) {
                        Toast.makeText(this@LoginActivity, "Login exitoso", Toast.LENGTH_SHORT).show()
                        val intent = Intent(this@LoginActivity, MainActivity::class.java)
                        startActivity(intent)
                        finish()
                    }
                } else {
                    withContext(Dispatchers.Main) {
                        Toast.makeText(this@LoginActivity, response.message ?: "Credenciales incorrectas", Toast.LENGTH_LONG).show()
                    }
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    Toast.makeText(this@LoginActivity, "Error: ${e.message}", Toast.LENGTH_LONG).show()
                }
            }
        }
    }
}