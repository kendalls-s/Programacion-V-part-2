using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CarnetDigitalWeb.Hubs
{
    public class TokenHub : Hub
    {
        public async Task UpdateToken(string token)
        {
            // Actualizar token en la sesión del cliente
            await Clients.Caller.SendAsync("TokenUpdated", token);
        }

        public async Task Logout()
        {
            await Clients.Caller.SendAsync("ForceLogout");
        }
    }
}