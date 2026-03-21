namespace itgsgroup.Hub
{
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class DataHub : Hub
    {
        // This method will be called by clients to receive updates.
        public async Task SendDataUpdate(string message)
        {
            // Broadcast the update to all connected clients.
            await Clients.All.SendAsync("ReceiveDataUpdate", message);
        }

        // You can add more methods here to handle different types of updates if needed.
    }

}
