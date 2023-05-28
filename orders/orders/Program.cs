using System.Data.Common;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<OrdersDataSource>();

var app = builder.Build();




app.MapPost("/orders",async (OrdersDataSource dataSource, Order order) =>
{

    order.Id = await dataSource.CreateOrder(order);
    return order;
});

app.MapPost("/orders/{orderid}/status/{status}", async (OrdersDataSource datasource, int orderid, string status) => {



    var statusId = (int)Enum.Parse(typeof(OrderStatus), status.ToLower());

    await datasource.UpdateStatus(orderid, statusId);
    
    return new { orderid, statusId };

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
    public OrderItem[] Items { get; set; }


}

class OrderItem
{
   

    public double Total => Quantity * Price;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set;  }
}


class OrdersDataSource
{

    private readonly NpgsqlDataSource datasource;

    public OrdersDataSource()
    {
        var connString = Environment.GetEnvironmentVariable("ORDERS_CONNECTION_STRING");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connString);
        datasource = dataSourceBuilder.Build();

        
    }

    public async Task<long> CreateOrder(Order order)
    {
        var conn = await datasource.OpenConnectionAsync();
        long id;

        await using (var cmd = new NpgsqlCommand("INSERT INTO orders (customer_id) VALUES (@customerId); SELECT lastval();", conn))
        {
            cmd.Parameters.AddWithValue("customerId", order.CustomerId);
            
            id=  (long)(cmd.ExecuteScalar());

        }


        foreach(OrderItem item in order.Items)
        {

            await using (var cmd = new NpgsqlCommand("INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(@orderid, @productid,@quantity,@price);",conn))
            {
                cmd.Parameters.AddWithValue("orderid", id);
                cmd.Parameters.AddWithValue("productid", item.ProductId);
                cmd.Parameters.AddWithValue("quantity", item.Quantity);
                cmd.Parameters.AddWithValue("price", item.Price);
                cmd.ExecuteNonQuery();

            }

        }
       

        conn.Close();

        return await Task.FromResult(id);

    }

    public async Task UpdateStatus(int orderId, int status)
    {
        var conn = await datasource.OpenConnectionAsync();
        

        await using (var cmd = new NpgsqlCommand("UPDATE orders set status = @status where id = @id", conn))
        {
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("id", orderId);
            cmd.ExecuteNonQuery();

        }

        conn.Close();

    }

}






