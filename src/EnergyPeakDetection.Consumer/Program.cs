using EnergyPeakDetection.Consumer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<SubmeteringStatsPeaksConsumerService>();
builder.Services.AddSingleton<SubmeteringPeaksInMemoryStore>();
builder.Services.AddSingleton<SubmeteringPeaksHubService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseEndpoints(conf => {
    conf.MapControllers();
    conf.MapHub<SubmeteringPeaksHub>("/peaksHub");
});

app.Run();
