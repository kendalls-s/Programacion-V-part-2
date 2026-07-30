using SRV13_Fotografia.Entities;
using SRV13_Fotografia.Repository;

namespace SRV13_Fotografia.Services
{
    public class FotografiaService : IFotografiaService
    {
        // HU SRV13: la imagen no debe ser superior a 1 MB
        private const int MaxBytes = 1024 * 1024;

        private readonly FotografiaRepository _repository;

        public FotografiaService(FotografiaRepository repository) { _repository = repository; }

        public async Task<string?> ObtenerFotografiaAsync(int usuarioId)
        {
            var bytes = await _repository.ObtenerAsync(usuarioId);
            return bytes is null || bytes.Length == 0 ? null : Convert.ToBase64String(bytes);
        }

        public async Task<(int, string?, FotografiaUsuario?)> ActualizarFotografiaAsync(int usuarioId, string fotografiaBase64)
        {
            if (!await _repository.ExisteUsuarioAsync(usuarioId))
                return (-1, null, null);

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(fotografiaBase64.Trim());
            }
            catch (FormatException)
            {
                return (-2, "La fotografia no esta en formato Base 64 valido", null);
            }

            if (bytes.Length == 0)
                return (-2, "La fotografia esta vacia", null);

            if (bytes.Length > MaxBytes)
                return (-2, "La fotografia no debe ser superior a 1 MB", null);

            var (ancho, alto) = ObtenerDimensiones(bytes);
            if (ancho <= 0 || alto <= 0)
                return (-2, "No fue posible determinar las dimensiones de la fotografia", null);

            if (ancho * 3 != alto * 4)
                return (-2, "La fotografia debe tener una relacion de aspecto 4:3", null);

            var filas = await _repository.ActualizarFotografiaAsync(usuarioId, bytes);
            if (filas <= 0) return (0, null, null);

            return (1, null, new FotografiaUsuario
            {
                UsuarioId = usuarioId,
                FotografiaBase64 = Convert.ToBase64String(bytes)
            });
        }

        // Lee ancho/alto desde los headers de la imagen (PNG o JPEG), sin librerias externas.
        private static (int ancho, int alto) ObtenerDimensiones(byte[] bytes)
        {
            // PNG: firma de 8 bytes + chunk IHDR (ancho en bytes 16-19, alto en bytes 20-23, big-endian)
            if (bytes.Length >= 24 &&
                bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                int ancho = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
                int alto = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
                return (ancho, alto);
            }

            // JPEG: recorrer los marcadores (segments) hasta encontrar uno SOF (Start Of Frame)
            if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xD8)
            {
                int i = 2;
                while (i + 9 < bytes.Length)
                {
                    if (bytes[i] != 0xFF) { i++; continue; }

                    byte marcador = bytes[i + 1];
                    bool esSOF = marcador >= 0xC0 && marcador <= 0xCF &&
                                 marcador != 0xC4 && marcador != 0xC8 && marcador != 0xCC;

                    if (esSOF)
                    {
                        int alto = (bytes[i + 5] << 8) | bytes[i + 6];
                        int ancho = (bytes[i + 7] << 8) | bytes[i + 8];
                        return (ancho, alto);
                    }

                    int longitudSegmento = (bytes[i + 2] << 8) | bytes[i + 3];
                    i += 2 + longitudSegmento;
                }
            }

            return (0, 0);
        }

        public async Task<(int, FotografiaUsuario?)> EliminarFotografiaAsync(int usuarioId)
        {
            if (!await _repository.ExisteUsuarioAsync(usuarioId))
                return (-1, null);

            var filas = await _repository.EliminarFotografiaAsync(usuarioId);
            if (filas <= 0) return (0, null);

            return (1, new FotografiaUsuario { UsuarioId = usuarioId, FotografiaBase64 = null });
        }
    }
}