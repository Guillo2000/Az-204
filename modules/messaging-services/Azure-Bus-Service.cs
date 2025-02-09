using Azure.Messaging.ServiceBus;
using ServiceBus;
using System.Text.Json;

string queueName = "appqueue";
string connectionString = "Endpoint=sb://servicebusapp2024gui.servicebus.windows.net/;SharedAccessKeyName=SendPolicy;SharedAccessKey=VxuhJtEFDIkXSuuz1LCIKd2RJN4xswvde+ASbA0t6LU=;EntityPath=appqueue";

List<Order> orders = new()
        {
            new Order { Id = 1, CourseName = "C# Avanzado", Price = 150.00m },
            new Order { Id = 2, CourseName = "SQL para Principiantes", Price = 99.99m },
            new Order { Id = 3, CourseName = "Angular desde Cero", Price = 120.50m }
        };

await SendMessages(orders);
await PeekMessages(3); 
await ReceiveMessages(3);

//Enviar Mensajes
async Task SendMessages(List<Order> orders)
{
    ServiceBusClient client = new(connectionString);
    ServiceBusSender sender = client.CreateSender(queueName);

    using ServiceBusMessageBatch serviceBusMessageBatch = await sender.CreateMessageBatchAsync();
    {
        foreach(Order order in orders)
        {
            ServiceBusMessage serviceBusMessage = new(JsonSerializer.Serialize(order));
            serviceBusMessage.ContentType = "application/json";
            serviceBusMessageBatch.TryAddMessage(serviceBusMessage);
        }
    }
    await sender.SendMessagesAsync(serviceBusMessageBatch);
    Console.WriteLine("Enviados");
    await sender.DisposeAsync();
    await client.DisposeAsync();
}

//Peek Mensajes. Recupera los mensajes de la cola pero no los elimina.
async Task PeekMessages(int numberOfMessages)
{
    ServiceBusClient client = new("Endpoint=sb://servicebusapp2024gui.servicebus.windows.net/;SharedAccessKeyName=ListenPolicy;SharedAccessKey=REJ9+8n4rBekuovSMaxk8xdkO87xHlpXV+ASbM3fWYc=;EntityPath=appqueue");
    ServiceBusReceiver receiver = client.CreateReceiver(queueName);

  IReadOnlyList<ServiceBusReceivedMessage> PeekMessages =  await receiver.PeekMessagesAsync(numberOfMessages);

    foreach(ServiceBusReceivedMessage message in PeekMessages)
    {
        Console.WriteLine($"Message id: {message.MessageId}");
    }

}

//Recibir mensajes. Recupera y los elimina
async Task ReceiveMessages(int numberOfMessages)
{
    ServiceBusClient client = new("Endpoint=sb://servicebusapp2024gui.servicebus.windows.net/;SharedAccessKeyName=ListenPolicy;SharedAccessKey=REJ9+8n4rBekuovSMaxk8xdkO87xHlpXV+ASbM3fWYc=;EntityPath=appqueue");
    ServiceBusReceiver receiver = client.CreateReceiver(queueName, 
    new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete});

    IReadOnlyList<ServiceBusReceivedMessage> ReceivedMessages = await receiver.ReceiveMessagesAsync(numberOfMessages);

    foreach (ServiceBusReceivedMessage message in ReceivedMessages)
    {
        Console.WriteLine($"Message id: {message.MessageId}");
    }

}