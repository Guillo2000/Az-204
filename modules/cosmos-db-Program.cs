
//Nos conectamos al cliente
using CosmosCRUD;
using Microsoft.Azure.Cosmos;

string connectionString = "AccountEndpoint=https://appcosmosdb204.documents.azure.com:443/;AccountKey=0APQCYOJYxCcbHu6pBtZ5aRteUpzBtzUQNRQ64RVQ4WWFpSM0d5rNGs8xXWRWUZ1mh1PGpzLr5G7ACDbEvoUAA==;";
CosmosClient client = new(connectionString);
Console.WriteLine($"Conectado al cliente");

string database = "appdb";

//Crear una base de datos en caso de que no exista. Tira una excepción si ya hay una con el mismo nombre
Database database1 = await client.CreateDatabaseIfNotExistsAsync(database);
Console.WriteLine($"Base de datos creada");

//Crear un contenedor:
string container = "courses";
string partitionKey = "/category";
await database1.CreateContainerIfNotExistsAsync(container, partitionKey);
Console.WriteLine("Contenedor creado");


//Crear un item dentro de un contenedor:
Container dbContainer = database1.GetContainer(container);
Course course = new()
{
    category = "Tecnologia",
    Name = "Az204",
    Price = 200,
    id = "course-1"
};
//Este metodo requiere que el item tenga un id, y una partition Key.
ItemResponse<Course> response = await dbContainer.CreateItemAsync<Course>(course, new PartitionKey(course.category));
Console.WriteLine($"Item creado: {response}");

//leer un item:
string id = "course-1";
string tecn = "Tecnologia";
ItemResponse<Course> readResponse = await dbContainer.ReadItemAsync<Course>(id, new PartitionKey(tecn));
Console.WriteLine($"Item: {readResponse}");

//Crea una query para items de un contenedor usando una clausula SQL con parametros. Returna un FeedIterator.

QueryDefinition query = new QueryDefinition(
    "SELECT * FROM courses s WHERE s.Price = @Price")
    .WithParameter("@Price", 200); 

FeedIterator<Course> resultSet = dbContainer.GetItemQueryIterator<Course>(
    query,
    requestOptions: new QueryRequestOptions()
    {
        PartitionKey = new PartitionKey("Tecnologia"), 
        MaxItemCount = 1
    });

//Eliminar un item. Se le pasa el id y la partition Key.
await DeleteItem("course-1", "Tecnologia");
async Task DeleteItem(string id, string category)
{
    CosmosClient cosmosClient = new(connectionString);

    string databaseId = "appdb";
    string containerId = "courses";

    Database database = cosmosClient.GetDatabase(databaseId);
    Container container = database.GetContainer(containerId);

    await container.DeleteItemAsync<Course>(id, new PartitionKey(category));

    Console.WriteLine("Course deleted");
}