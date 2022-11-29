using MassTransit;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;
using Play.Common.MongoDB;
using Play.Common.Settings;


var builder = WebApplication.CreateBuilder(args);

var serttings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddMongo().AddMongoRepository<Item>("items");

builder.Services.AddMassTransit(options =>
{
    options.UsingRabbitMq((context, configurator) =>
    {
        var rabbitMQSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
        configurator.Host(rabbitMQSettings.Host);
        configurator.ConfigureEndpoint(context, new KebabCaseEndpointNameFormatter(""));
    });
});

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Play.Catalog.Service", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
