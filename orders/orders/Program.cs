using System.Data.Common;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services
    .AddScoped<OrdersRepository>()
    .AddScoped<OrderItemRepository>()
    .AddScoped<OrderService>()
    .AddScoped<NpgsqlConnection>((context) =>
{
    var connString = Environment.GetEnvironmentVariable("ORDERS_CONNECTION_STRING");

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
   var dataSource = dataSourceBuilder.Build();
    return dataSource.OpenConnection();
});

var app = builder.Build();




app.MapPost("/orders",async (OrderService service, Order order) =>
{

    await service.InsertOrder(order);
    return order;
});

app.MapPost("/orders/{orderid}/status/{status}", async (OrderService service, int orderid, string status) => {



    var statusId = (int)Enum.Parse(typeof(OrderStatus), status.ToLower());

    await service.UpdateStatus(orderid, statusId);
    
    return new { orderid, statusId };

});

app.MapGet("/orders/{id}/status", async (OrderService service, int id) =>
{
    var status = await service.GetOrderStatus(id);   
    return new { orderId=id, status = new { id = status, name = status.ToString() } };
});



app.Run();

public enum OrderStatus
{
    preparing = 1,
    transit = 2,
    delivered = 3
}



class Order
{

    public long Id { get; set; }
    public int CustomerId { get; set;  }
    public OrderItem[]? Items { get; set; }


}

class OrderItem
{
   
    public double Total => Quantity * Price;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set;  }
}

abstract class Repository : IDisposable
{
    protected NpgsqlConnection Connection { get; }

    public Repository(NpgsqlConnection connection) => Connection = connection;

    public void Dispose()
    {
        Connection.Close();
    }
}

class OrderService
{
    private readonly OrdersRepository ordersRepository;
    private readonly OrderItemRepository itemRepository;

    public OrderService(OrdersRepository ordersRepository, OrderItemRepository itemRepository)
    {
        this.ordersRepository = ordersRepository;
        this.itemRepository = itemRepository;
    }

    public async Task InsertOrder(Order order)
    {

        order.Id = await ordersRepository.Insert(order);


        foreach (OrderItem item in order.Items)
            await itemRepository.Insert(order.Id, item);


    }


    public async Task UpdateStatus(long orderId, int status) => await ordersRepository.UpdateStatus(orderId, status);

    public async Task<OrderStatus?> GetOrderStatus(int orderid) => await ordersRepository.GetStatus(orderid);
}

class OrderItemRepository : Repository
{
    public OrderItemRepository(NpgsqlConnection connection) : base(connection)
    {
    }

    public async Task Insert(long orderId, OrderItem item)
    {
        await using (var cmd = new NpgsqlCommand("INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(@orderid, @productid,@quantity,@price);", base.Connection))
        {
            cmd.Parameters.AddWithValue("orderid", orderId);
            cmd.Parameters.AddWithValue("productid", item.ProductId);
            cmd.Parameters.AddWithValue("quantity", item.Quantity);
            cmd.Parameters.AddWithValue("price", item.Price);
            cmd.ExecuteNonQuery();

        }

    }
}

class OrdersRepository : Repository
{

    public OrdersRepository(NpgsqlConnection connection) : base(connection)
    {
        
    }

    

    public async Task<long> Insert(Order order)
    {
        
        await using (var cmd = new NpgsqlCommand("INSERT INTO orders (customer_id) VALUES (@customerId); SELECT lastval();", base.Connection))
        {
            cmd.Parameters.AddWithValue("customerId", order.CustomerId);

            var result = cmd.ExecuteScalar();

            if (result == null)
                throw new NullReferenceException("Null value has returned from database");
            else
                return (long)result;

        }

    }

    public async Task UpdateStatus(long orderId, int status)
    {

        await using (var cmd = new NpgsqlCommand("UPDATE orders set status = @status where id = @id", base.Connection))
        {
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("id", orderId);
            cmd.ExecuteNonQuery();

        }

    }

    public async Task<OrderStatus?> GetStatus(int orderid)
    {
        await using (var cmd = new NpgsqlCommand("SELECT status FROM orders WHERE id = @id", base.Connection))
        {
            cmd.Parameters.AddWithValue("id", orderid);

            var result = cmd.ExecuteScalar();

            return result == null ? null : (OrderStatus)result;
        }

    }
}






