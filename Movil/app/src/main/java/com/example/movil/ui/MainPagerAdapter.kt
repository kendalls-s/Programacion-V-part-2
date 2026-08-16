package com.example.movil.ui

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.example.movil.R

/**
 * Adapter del ViewPager2 de la pantalla principal (USR2 / GRD2).
 *
 * Página 0: datos + foto del usuario (fragment_datos_usuario.xml).
 * Página 1: placeholder en blanco de la sección de QR. Solo muestra
 * un título ("Foto QR" para usuarios, "Scan QR" para guardas); el
 * contenido real se implementará en otra historia de usuario
 * (USR3 / GRD3).
 */
class MainPagerAdapter(
    private val tituloQr: String,
    private val onDatosViewCreated: (View) -> Unit
) : RecyclerView.Adapter<RecyclerView.ViewHolder>() {

    private class SimpleViewHolder(view: View) : RecyclerView.ViewHolder(view)

    override fun getItemViewType(position: Int): Int = position

    override fun getItemCount(): Int = 2

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecyclerView.ViewHolder {
        val inflater = LayoutInflater.from(parent.context)
        return if (viewType == 0) {
            val view = inflater.inflate(R.layout.fragment_datos_usuario, parent, false)
            onDatosViewCreated(view)
            SimpleViewHolder(view)
        } else {
            val view = inflater.inflate(R.layout.fragment_qr_placeholder, parent, false)
            view.findViewById<TextView>(R.id.tvTituloQr).text = tituloQr
            SimpleViewHolder(view)
        }
    }

    override fun onBindViewHolder(holder: RecyclerView.ViewHolder, position: Int) {
        // Páginas fijas (sin reciclaje real): el contenido ya se
        // configuró en onCreateViewHolder.
    }
}
