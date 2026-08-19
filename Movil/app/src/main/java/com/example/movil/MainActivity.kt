package com.example.movil

import android.Manifest
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.graphics.Color
import android.media.AudioManager
import android.media.ToneGenerator
import android.os.Bundle
import android.util.Base64
import android.view.View
import android.widget.Button
import android.widget.ImageButton
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.camera.core.CameraSelector
import androidx.camera.core.ExperimentalGetImage
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import androidx.viewpager2.widget.ViewPager2
import com.example.movil.model.CarnetQrData
import com.example.movil.model.FotografiaResponse
import com.example.movil.model.Usuario
import com.example.movil.network.ApiClient
import com.example.movil.ui.MainPagerAdapter
import com.google.gson.Gson
import com.google.mlkit.vision.barcode.BarcodeScanner
import com.google.mlkit.vision.barcode.BarcodeScannerOptions
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.barcode.common.Barcode
import com.google.mlkit.vision.common.InputImage
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import java.util.concurrent.Executors

@ExperimentalGetImage
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

    private var esGuarda = false

    private var scannerRoot: View? = null
    private var scannerIniciado = false
    private var procesandoFrame = false

    private val qrRecientes =
        mutableMapOf<String, Long>()

    private val tiempoRepeticionQr =
        1000L

    private val cameraExecutor =
        Executors.newSingleThreadExecutor()

    private val opcionesQr by lazy {
        BarcodeScannerOptions
            .Builder()
            .setBarcodeFormats(
                Barcode.FORMAT_QR_CODE
            )
            .build()
    }

    private val scannerCamara: BarcodeScanner by lazy {
        BarcodeScanning.getClient(
            opcionesQr
        )
    }

    private val scannerValidacion: BarcodeScanner by lazy {
        BarcodeScanning.getClient(
            opcionesQr
        )
    }

    private val solicitudCamara =
        registerForActivityResult(
            ActivityResultContracts.RequestPermission()
        ) { permitido ->

            if (permitido) {

                scannerRoot?.let {
                    iniciarCamara(it)
                }

            } else {

                scannerRoot?.let {

                    mostrarResultadoScanner(
                        it,
                        false,
                        "Permiso de cámara denegado"
                    )
                }
            }
        }

    override fun onCreate(
        savedInstanceState: Bundle?
    ) {

        super.onCreate(savedInstanceState)

        setContentView(
            R.layout.activity_main
        )

        val prefs =
            getSharedPreferences(
                "AppPrefs",
                Context.MODE_PRIVATE
            )

        val token =
            prefs.getString(
                "token",
                null
            )

        val userId =
            prefs.getString(
                "userId",
                null
            )

        if (
            token == null ||
            userId == null
        ) {

            startActivity(
                Intent(
                    this,
                    LoginActivity::class.java
                )
            )

            finish()
            return
        }

        ApiClient.token = token

        viewPager =
            findViewById(
                R.id.viewPager
            )

        progressBar =
            findViewById(
                R.id.progressBar
            )

        tvError =
            findViewById(
                R.id.tvError
            )

        cargarUsuario(
            userId
        )
    }

    private fun cargarUsuario(
        id: String
    ) {

        mostrarCargando(
            true
        )

        lifecycleScope.launch {

            try {

                val usuario =
                    ApiClient.usuarioApi
                        .obtenerUsuario(id)

                mostrarCargando(
                    false
                )

                configurarPager(
                    usuario,
                    id
                )

            } catch (e: Exception) {

                e.printStackTrace()

                mostrarCargando(
                    false
                )

                mostrarError()
            }
        }
    }

    private fun configurarPager(
        usuario: Usuario,
        idUsuario: String
    ) {

        esGuarda =
            usuario.tipoUsuario.equals(
                "Guarda",
                ignoreCase = true
            )

        val tituloQr =
            if (esGuarda) {

                getString(
                    R.string.label_scan_qr
                )

            } else {

                getString(
                    R.string.label_foto_qr
                )
            }

        viewPager.isUserInputEnabled =
            false

        viewPager.adapter =
            MainPagerAdapter(

                esGuarda = esGuarda,

                tituloQr = tituloQr,

                onDatosViewCreated = {
                        vistaDatos ->

                    bindDatosView(
                        vistaDatos,
                        usuario,
                        esGuarda
                    )

                    cargarFotografia(
                        idUsuario
                    )
                },

                onQrViewCreated = {
                        vistaQr ->

                    if (!esGuarda) {

                        cargarQr(
                            usuario.numeroIdentificacion,
                            vistaQr
                        )
                    }
                },

                onScannerViewCreated = {
                        vistaScanner ->

                    if (esGuarda) {

                        scannerRoot =
                            vistaScanner
                    }
                }
            )
    }

    private fun bindDatosView(
        root: View,
        usuario: Usuario,
        esGuarda: Boolean
    ) {

        ivFoto =
            root.findViewById(
                R.id.ivFoto
            )

        tvLeyendaSinFoto =
            root.findViewById(
                R.id.tvLeyendaSinFoto
            )

        tvNombreCompleto =
            root.findViewById(
                R.id.tvNombreCompleto
            )

        tvTipoUsuario =
            root.findViewById(
                R.id.tvTipoUsuario
            )

        tvIdentificacion =
            root.findViewById(
                R.id.tvIdentificacion
            )

        groupCarreraArea =
            root.findViewById(
                R.id.groupCarreraArea
            )

        tvCarreraArea =
            root.findViewById(
                R.id.tvCarreraArea
            )

        btnCerrarSesion =
            root.findViewById(
                R.id.btnCerrarSesion
            )

        btnCamara =
            root.findViewById(
                R.id.btnCamara
            )

        tvNombreCompleto.text =
            usuario.nombreCompleto

        tvIdentificacion.text =
            usuario.numeroIdentificacion

        tvTipoUsuario.text =
            usuario.tipoUsuario

        if (esGuarda) {

            groupCarreraArea.visibility =
                View.GONE

            btnCamara.visibility =
                View.VISIBLE

            btnCamara.isEnabled =
                false

            btnCamara.setOnClickListener {

                viewPager.currentItem =
                    1

                viewPager.postDelayed({

                    scannerRoot?.let {

                        solicitarCamara(
                            it
                        )
                    }

                }, 150)
            }

        } else {

            groupCarreraArea.visibility =
                View.VISIBLE

            tvCarreraArea.text =
                usuario.carreraOArea()

            btnCamara.visibility =
                View.GONE
        }

        btnCerrarSesion.setOnClickListener {

            val prefs =
                getSharedPreferences(
                    "AppPrefs",
                    Context.MODE_PRIVATE
                )

            prefs.edit()
                .clear()
                .apply()

            ApiClient.token =
                null

            val intent =
                Intent(
                    this,
                    LoginActivity::class.java
                )

            intent.flags =
                Intent.FLAG_ACTIVITY_NEW_TASK or
                        Intent.FLAG_ACTIVITY_CLEAR_TASK

            startActivity(
                intent
            )

            finish()
        }
    }

    private fun cargarFotografia(
        idUsuario: String
    ) {

        lifecycleScope.launch {

            try {

                val response =
                    ApiClient.usuarioApi
                        .obtenerFotografia(
                            idUsuario
                        )

                val body =
                    response
                        .string()
                        .trim()

                if (
                    body.isBlank()
                ) {

                    mostrarFotoNoDisponible()
                    return@launch
                }

                val crudo =
                    when {

                        body.startsWith("{") -> {

                            val obj =
                                Gson().fromJson(
                                    body,
                                    FotografiaResponse::class.java
                                )

                            obj?.fotografiaBase64
                        }

                        body.startsWith("\"") &&
                                body.endsWith("\"") -> {

                            Gson().fromJson(
                                body,
                                String::class.java
                            )
                        }

                        else -> body
                    }

                val base64 =
                    limpiarBase64(
                        crudo ?: ""
                    )

                if (
                    base64.isBlank()
                ) {

                    mostrarFotoNoDisponible()
                    return@launch
                }

                val imageBytes =
                    Base64.decode(
                        base64,
                        Base64.DEFAULT
                    )

                val bitmap =
                    BitmapFactory.decodeByteArray(
                        imageBytes,
                        0,
                        imageBytes.size
                    )

                if (
                    bitmap != null
                ) {

                    ivFoto.setImageBitmap(
                        bitmap
                    )

                    tvLeyendaSinFoto.visibility =
                        View.GONE

                    if (esGuarda) {

                        btnCamara.isEnabled =
                            true

                    } else {

                        viewPager
                            .isUserInputEnabled =
                            true
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

    private fun cargarQr(
        identificacion: String,
        root: View
    ) {

        val ivCodigoQr =
            root.findViewById<ImageView>(
                R.id.ivCodigoQr
            )

        val progressQr =
            root.findViewById<ProgressBar>(
                R.id.progressQr
            )

        val tvErrorQr =
            root.findViewById<TextView>(
                R.id.tvErrorQr
            )

        progressQr.visibility =
            View.VISIBLE

        tvErrorQr.visibility =
            View.GONE

        ivCodigoQr.visibility =
            View.INVISIBLE

        lifecycleScope.launch {

            try {

                val response =
                    ApiClient.usuarioApi
                        .obtenerQr(
                            identificacion
                        )

                val body =
                    response
                        .string()
                        .trim()

                if (
                    body.isBlank()
                ) {

                    mostrarErrorQr(
                        ivCodigoQr,
                        progressQr,
                        tvErrorQr
                    )

                    return@launch
                }

                val crudo =
                    obtenerTextoRespuesta(
                        body
                    )

                val base64 =
                    limpiarBase64(
                        crudo
                    )

                if (
                    base64.isBlank()
                ) {

                    mostrarErrorQr(
                        ivCodigoQr,
                        progressQr,
                        tvErrorQr
                    )

                    return@launch
                }

                val qrBytes =
                    Base64.decode(
                        base64,
                        Base64.DEFAULT
                    )

                val bitmap =
                    BitmapFactory.decodeByteArray(
                        qrBytes,
                        0,
                        qrBytes.size
                    )

                if (
                    bitmap != null
                ) {

                    ivCodigoQr
                        .setImageBitmap(
                            bitmap
                        )

                    ivCodigoQr.visibility =
                        View.VISIBLE

                    progressQr.visibility =
                        View.GONE

                    tvErrorQr.visibility =
                        View.GONE

                } else {

                    mostrarErrorQr(
                        ivCodigoQr,
                        progressQr,
                        tvErrorQr
                    )
                }

            } catch (e: Exception) {

                e.printStackTrace()

                mostrarErrorQr(
                    ivCodigoQr,
                    progressQr,
                    tvErrorQr
                )
            }
        }
    }

    private fun solicitarCamara(
        root: View
    ) {

        scannerRoot =
            root

        if (
            ContextCompat.checkSelfPermission(
                this,
                Manifest.permission.CAMERA
            ) ==
            PackageManager.PERMISSION_GRANTED
        ) {

            iniciarCamara(
                root
            )

        } else {

            solicitudCamara.launch(
                Manifest.permission.CAMERA
            )
        }
    }

    private fun iniciarCamara(
        root: View
    ) {

        if (
            scannerIniciado
        ) {
            return
        }

        scannerIniciado =
            true

        val previewView =
            root.findViewById<PreviewView>(
                R.id.previewCamara
            )

        val cameraProviderFuture =
            ProcessCameraProvider
                .getInstance(this)

        cameraProviderFuture.addListener({

            try {

                val cameraProvider =
                    cameraProviderFuture.get()

                val preview =
                    Preview.Builder()
                        .build()

                preview.setSurfaceProvider(
                    previewView.surfaceProvider
                )

                val imageAnalysis =
                    ImageAnalysis.Builder()
                        .setBackpressureStrategy(
                            ImageAnalysis
                                .STRATEGY_KEEP_ONLY_LATEST
                        )
                        .build()

                imageAnalysis.setAnalyzer(
                    cameraExecutor
                ) { imageProxy ->

                    if (
                        procesandoFrame
                    ) {

                        imageProxy.close()
                        return@setAnalyzer
                    }

                    val mediaImage =
                        imageProxy.image

                    if (
                        mediaImage == null
                    ) {

                        imageProxy.close()
                        return@setAnalyzer
                    }

                    procesandoFrame =
                        true

                    val image =
                        InputImage.fromMediaImage(
                            mediaImage,
                            imageProxy
                                .imageInfo
                                .rotationDegrees
                        )

                    scannerCamara
                        .process(image)

                        .addOnSuccessListener {
                                barcodes ->

                            for (
                            barcode in barcodes
                            ) {

                                val contenido =
                                    barcode.rawValue

                                if (
                                    !contenido
                                        .isNullOrBlank()
                                ) {

                                    runOnUiThread {

                                        procesarQrDetectado(
                                            contenido,
                                            root
                                        )
                                    }
                                }
                            }
                        }

                        .addOnCompleteListener {

                            procesandoFrame =
                                false

                            imageProxy.close()
                        }
                }

                cameraProvider.unbindAll()

                cameraProvider.bindToLifecycle(
                    this,
                    CameraSelector
                        .DEFAULT_BACK_CAMERA,
                    preview,
                    imageAnalysis
                )

            } catch (e: Exception) {

                e.printStackTrace()

                scannerIniciado =
                    false

                mostrarResultadoScanner(
                    root,
                    false,
                    "No se pudo iniciar la cámara"
                )
            }

        }, ContextCompat.getMainExecutor(this))
    }

    private fun procesarQrDetectado(
        contenido: String,
        root: View
    ) {

        val ahora =
            System.currentTimeMillis()

        val ultimaLectura =
            qrRecientes[
                contenido
            ]

        if (
            ultimaLectura != null &&
            ahora - ultimaLectura <
            tiempoRepeticionQr
        ) {

            return
        }

        qrRecientes[
            contenido
        ] =
            ahora

        limpiarQrRecientes(
            ahora
        )

        try {

            val qrEscaneado =
                Gson().fromJson(
                    contenido,
                    CarnetQrData::class.java
                )

            if (
                qrEscaneado
                    .identificacion
                    .isBlank()
            ) {

                resultadoInvalido(
                    root,
                    "El código QR no contiene una identificación válida."
                )

                return
            }

            mostrarValidando(
                root,
                qrEscaneado
            )

            validarQrContraServidor(
                qrEscaneado,
                root
            )

        } catch (e: Exception) {

            e.printStackTrace()

            resultadoInvalido(
                root,
                "El código escaneado no es un QR institucional válido."
            )
        }
    }

    private fun limpiarQrRecientes(
        ahora: Long
    ) {

        val limite =
            ahora - 5000L

        val iterator =
            qrRecientes
                .entries
                .iterator()

        while (
            iterator.hasNext()
        ) {

            val entrada =
                iterator.next()

            if (
                entrada.value <
                limite
            ) {

                iterator.remove()
            }
        }
    }

    private fun validarQrContraServidor(
        qrEscaneado: CarnetQrData,
        root: View
    ) {

        lifecycleScope.launch {

            try {

                val response =
                    ApiClient.usuarioApi
                        .obtenerQr(
                            qrEscaneado.identificacion
                        )

                val body =
                    response
                        .string()
                        .trim()

                if (
                    body.isBlank()
                ) {

                    resultadoInvalido(
                        root,
                        "No fue posible validar el usuario."
                    )

                    return@launch
                }

                val crudo =
                    obtenerTextoRespuesta(
                        body
                    )

                val base64 =
                    limpiarBase64(
                        crudo
                    )

                if (
                    base64.isBlank()
                ) {

                    resultadoInvalido(
                        root,
                        "No fue posible validar el usuario."
                    )

                    return@launch
                }

                val qrBytes =
                    Base64.decode(
                        base64,
                        Base64.DEFAULT
                    )

                val bitmap =
                    BitmapFactory.decodeByteArray(
                        qrBytes,
                        0,
                        qrBytes.size
                    )

                if (
                    bitmap == null
                ) {

                    resultadoInvalido(
                        root,
                        "No fue posible validar el QR."
                    )

                    return@launch
                }

                leerQrOficial(
                    bitmap,
                    qrEscaneado,
                    root
                )

            } catch (e: Exception) {

                e.printStackTrace()

                resultadoInvalido(
                    root,
                    "No se encontró un usuario válido para este QR."
                )
            }
        }
    }

    private fun leerQrOficial(
        bitmap: Bitmap,
        qrEscaneado: CarnetQrData,
        root: View
    ) {

        val image =
            InputImage.fromBitmap(
                bitmap,
                0
            )

        scannerValidacion
            .process(image)

            .addOnSuccessListener {
                    barcodes ->

                val contenidoOficial =
                    barcodes
                        .firstOrNull {
                            !it.rawValue
                                .isNullOrBlank()
                        }
                        ?.rawValue

                if (
                    contenidoOficial
                        .isNullOrBlank()
                ) {

                    resultadoInvalido(
                        root,
                        "No fue posible leer el QR oficial."
                    )

                    return@addOnSuccessListener
                }

                try {

                    val qrOficial =
                        Gson().fromJson(
                            contenidoOficial,
                            CarnetQrData::class.java
                        )

                    if (
                        compararQr(
                            qrEscaneado,
                            qrOficial
                        )
                    ) {

                        resultadoValido(
                            root,
                            qrOficial
                        )

                    } else {

                        resultadoInvalido(
                            root,
                            "Los datos del QR no coinciden con la información registrada."
                        )
                    }

                } catch (e: Exception) {

                    e.printStackTrace()

                    resultadoInvalido(
                        root,
                        "El contenido del QR no es válido."
                    )
                }
            }

            .addOnFailureListener {

                resultadoInvalido(
                    root,
                    "No fue posible validar el QR."
                )
            }
    }

    private fun compararQr(
        escaneado: CarnetQrData,
        oficial: CarnetQrData
    ): Boolean {

        val carrerasEscaneadas =
            escaneado
                .carrerasOAreas
                .map {
                    it.trim()
                }
                .sorted()

        val carrerasOficiales =
            oficial
                .carrerasOAreas
                .map {
                    it.trim()
                }
                .sorted()

        return escaneado
            .nombreCompleto
            .trim()
            .equals(
                oficial
                    .nombreCompleto
                    .trim(),
                ignoreCase = true
            ) &&

                escaneado
                    .identificacion
                    .trim() ==
                oficial
                    .identificacion
                    .trim() &&

                escaneado
                    .tipoUsuario
                    .trim()
                    .equals(
                        oficial
                            .tipoUsuario
                            .trim(),
                        ignoreCase = true
                    ) &&

                carrerasEscaneadas ==
                carrerasOficiales &&

                escaneado
                    .institucion
                    .trim()
                    .equals(
                        oficial
                            .institucion
                            .trim(),
                        ignoreCase = true
                    ) &&

                escaneado
                    .fechaVencimiento
                    .trim() ==
                oficial
                    .fechaVencimiento
                    .trim()
    }

    private fun mostrarValidando(
        root: View,
        qr: CarnetQrData
    ) {

        val card =
            root.findViewById<View>(
                R.id.cardResultadoScanner
            )

        val contenedor =
            root.findViewById<LinearLayout>(
                R.id.contenedorResultadoScanner
            )

        val icono =
            root.findViewById<TextView>(
                R.id.tvIconoResultado
            )

        val titulo =
            root.findViewById<TextView>(
                R.id.tvResultadoScanner
            )

        val detalle =
            root.findViewById<TextView>(
                R.id.tvDetalleScanner
            )

        card.visibility =
            View.VISIBLE

        contenedor.setBackgroundColor(
            Color.rgb(
                239,
                246,
                255
            )
        )

        icono.text =
            "⌛"

        icono.setTextColor(
            Color.rgb(
                37,
                99,
                235
            )
        )

        titulo.text =
            "VALIDANDO..."

        titulo.setTextColor(
            Color.rgb(
                30,
                64,
                175
            )
        )

        detalle.text =
            qr.identificacion

        detalle.setTextColor(
            Color.rgb(
                30,
                64,
                175
            )
        )
    }

    private fun resultadoValido(
        root: View,
        qr: CarnetQrData
    ) {

        reproducirSonido(
            true
        )

        mostrarResultadoScanner(
            root,
            true,
            "${qr.nombreCompleto}\nIdentificación: ${qr.identificacion}"
        )
    }

    private fun resultadoInvalido(
        root: View,
        mensaje: String
    ) {

        reproducirSonido(
            false
        )

        mostrarResultadoScanner(
            root,
            false,
            mensaje
        )
    }

    private fun mostrarResultadoScanner(
        root: View,
        valido: Boolean,
        detalle: String
    ) {

        val card =
            root.findViewById<View>(
                R.id.cardResultadoScanner
            )

        val contenedor =
            root.findViewById<LinearLayout>(
                R.id.contenedorResultadoScanner
            )

        val icono =
            root.findViewById<TextView>(
                R.id.tvIconoResultado
            )

        val titulo =
            root.findViewById<TextView>(
                R.id.tvResultadoScanner
            )

        val textoDetalle =
            root.findViewById<TextView>(
                R.id.tvDetalleScanner
            )

        card.visibility =
            View.VISIBLE

        if (valido) {

            contenedor.setBackgroundColor(
                Color.rgb(
                    220,
                    252,
                    231
                )
            )

            icono.text =
                "✓"

            icono.setTextColor(
                Color.rgb(
                    22,
                    163,
                    74
                )
            )

            titulo.text =
                "QR VÁLIDO"

            titulo.setTextColor(
                Color.rgb(
                    22,
                    101,
                    52
                )
            )

            textoDetalle.setTextColor(
                Color.rgb(
                    22,
                    101,
                    52
                )
            )

        } else {

            contenedor.setBackgroundColor(
                Color.rgb(
                    254,
                    226,
                    226
                )
            )

            icono.text =
                "✕"

            icono.setTextColor(
                Color.rgb(
                    220,
                    38,
                    38
                )
            )

            titulo.text =
                "QR INVÁLIDO"

            titulo.setTextColor(
                Color.rgb(
                    153,
                    27,
                    27
                )
            )

            textoDetalle.setTextColor(
                Color.rgb(
                    153,
                    27,
                    27
                )
            )
        }

        textoDetalle.text =
            detalle
    }

    private fun reproducirSonido(
        valido: Boolean
    ) {

        try {

            if (valido) {

                val tono =
                    ToneGenerator(
                        AudioManager.STREAM_MUSIC,
                        100
                    )

                tono.startTone(
                    ToneGenerator.TONE_PROP_ACK,
                    350
                )

                lifecycleScope.launch {

                    delay(400)

                    tono.release()
                }

            } else {

                val tono =
                    ToneGenerator(
                        AudioManager.STREAM_MUSIC,
                        100
                    )

                tono.startTone(
                    ToneGenerator.TONE_PROP_NACK,
                    450
                )

                lifecycleScope.launch {

                    delay(500)

                    tono.release()
                }
            }

        } catch (e: Exception) {

            e.printStackTrace()
        }
    }

    private fun obtenerTextoRespuesta(
        body: String
    ): String {

        return if (
            body.startsWith("\"") &&
            body.endsWith("\"")
        ) {

            Gson().fromJson(
                body,
                String::class.java
            )

        } else {

            body
        }
    }

    private fun limpiarBase64(
        valor: String
    ): String {

        return valor
            .trim()
            .replaceFirst(
                Regex(
                    "^data:image/[^;]+;base64,"
                ),
                ""
            )
            .replace("\n", "")
            .replace("\r", "")
    }

    private fun mostrarErrorQr(
        ivCodigoQr: ImageView,
        progressQr: ProgressBar,
        tvErrorQr: TextView
    ) {

        progressQr.visibility =
            View.GONE

        ivCodigoQr.visibility =
            View.INVISIBLE

        tvErrorQr.text =
            "No se pudo cargar el código QR."

        tvErrorQr.visibility =
            View.VISIBLE
    }

    private fun mostrarFotoNoDisponible() {

        ivFoto.setImageResource(
            R.drawable.ic_avatar_placeholder
        )

        tvLeyendaSinFoto.text =
            getString(
                R.string.leyenda_sin_foto
            )

        tvLeyendaSinFoto.visibility =
            View.VISIBLE

        if (esGuarda) {

            btnCamara.isEnabled =
                false

        } else {

            viewPager.isUserInputEnabled =
                false
        }
    }

    private fun mostrarCargando(
        cargando: Boolean
    ) {

        progressBar.visibility =
            if (cargando) {

                View.VISIBLE

            } else {

                View.GONE
            }

        tvError.visibility =
            View.GONE
    }

    private fun mostrarError() {

        tvError.text =
            getString(
                R.string.error_carga_usuario
            )

        tvError.visibility =
            View.VISIBLE
    }

    override fun onDestroy() {

        scannerCamara.close()
        scannerValidacion.close()

        cameraExecutor.shutdown()

        super.onDestroy()
    }
}