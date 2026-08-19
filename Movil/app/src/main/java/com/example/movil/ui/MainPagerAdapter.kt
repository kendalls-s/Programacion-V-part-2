package com.example.movil.ui

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView
import com.example.movil.R

class MainPagerAdapter(
    private val esGuarda: Boolean,
    private val tituloQr: String,
    private val onDatosViewCreated: (View) -> Unit,
    private val onQrViewCreated: (View) -> Unit,
    private val onScannerViewCreated: (View) -> Unit
) : RecyclerView.Adapter<RecyclerView.ViewHolder>() {

    private class SimpleViewHolder(
        view: View
    ) : RecyclerView.ViewHolder(view)

    override fun getItemViewType(position: Int): Int {
        return position
    }

    override fun getItemCount(): Int {
        return 2
    }

    override fun onCreateViewHolder(
        parent: ViewGroup,
        viewType: Int
    ): RecyclerView.ViewHolder {

        val inflater = LayoutInflater.from(parent.context)

        return if (viewType == 0) {

            val view = inflater.inflate(
                R.layout.fragment_datos_usuario,
                parent,
                false
            )

            onDatosViewCreated(view)

            SimpleViewHolder(view)

        } else {

            if (esGuarda) {

                val view = inflater.inflate(
                    R.layout.fragment_scanner_qr,
                    parent,
                    false
                )

                onScannerViewCreated(view)

                SimpleViewHolder(view)

            } else {

                val view = inflater.inflate(
                    R.layout.fragment_qr_placeholder,
                    parent,
                    false
                )

                view.findViewById<TextView>(
                    R.id.tvTituloQr
                ).text = tituloQr

                onQrViewCreated(view)

                SimpleViewHolder(view)
            }
        }
    }

    override fun onBindViewHolder(
        holder: RecyclerView.ViewHolder,
        position: Int
    ) {
    }
}