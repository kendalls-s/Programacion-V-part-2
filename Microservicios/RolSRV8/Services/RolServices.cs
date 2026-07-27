using RolSRV8.Entities;
using RolSRV8.Repository;

namespace RolSRV8.Services;

public class RolService : IRolService
{
    private readonly RolRepository _repository;


    public RolService(RolRepository repository)
    {
        _repository = repository;
    }



    public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
    {
        return await _repository.ObtenerTodosAsync();
    }



    public async Task<Rol?> ObtenerPorIdAsync(int id)
    {
        return await _repository.ObtenerPorIdAsync(id);
    }




    public async Task<(bool ok, string error, int id)> CrearAsync(
    RolRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (false, "El nombre es requerido", 0);


        if (request.Pantallas == null || request.Pantallas.Count == 0)
            return (false, "Debe seleccionar pantallas", 0);



        var rol = new Rol
        {
            Nombre = request.Nombre
        };


        var id = await _repository.CrearAsync(
            rol,
            request.Pantallas);



        return (true, string.Empty, id);
    }






    public async Task<(bool ok, string error)> ActualizarAsync(
        int id,
        RolRequest request)
    {

        var rol = await _repository.ObtenerPorIdAsync(id);



        if (rol == null)
        {
            return (
                false,
                "Rol no encontrado");
        }



        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            return (
                false,
                "El nombre es requerido");
        }



        if (request.Pantallas == null ||
            request.Pantallas.Count == 0)
        {
            return (
                false,
                "Debe seleccionar al menos una pantalla");
        }




        await _repository.ActualizarAsync(
            id,
            request.Nombre,
            request.Pantallas);



        return (
            true,
            string.Empty);
    }






    public async Task<(bool ok, string error)> EliminarAsync(int id)
    {

        var rol =
            await _repository.ObtenerPorIdAsync(id);



        if (rol == null)
        {
            return (
                false,
                "Rol no encontrado");
        }




        var cantidad =
            await _repository.ContarUsuariosAsync(id);



        if (cantidad > 0)
        {
            return (
                false,
                $"No se puede eliminar el rol porque está asignado a {cantidad} usuario(s)");
        }




        await _repository.EliminarAsync(id);



        return (
            true,
            string.Empty);
    }





    public async Task<int> ContarUsuariosAsync(int id)
    {
        return await _repository.ContarUsuariosAsync(id);
    }
}