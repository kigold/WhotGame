using Orleans;
using Orleans.Hosting;
using WhotGame.Grains;
using WhotGame.Silo;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((ctx, orleansBuilder) =>
{
    if (ctx.HostingEnvironment.IsDevelopment())
    {
        // During development time, we don't want to have to deal with
        // storage emulators or other dependencies. Just "Hit F5" to run.
        orleansBuilder
            .UseLocalhostClustering(/*gatewayPort: 30000*/)
            .AddMemoryGrainStorage("WhotGame")
            .UseInMemoryReminderService()
            //.ConfigureApplicationParts(x =>
            //{
            //    x.AddApplicationPart(typeof(GameGrain).Assembly).WithReferences();
            //    x.AddApplicationPart(typeof(PlayerGrain).Assembly).WithReferences();
            //})
            .UseDashboard(options =>  options.Port = 8000);
    }
    else
    {
        // In Kubernetes, we use environment variables and the pod manifest
        //orleansBuilder.UseKubernetesHosting();

        // Use Redis for clustering & persistence
        //var redisAddress = $"{Environment.GetEnvironmentVariable("REDIS")}:6379";
        //orleansBuilder.UseRedisClustering(options => options.ConnectionString = redisAddress);
        //orleansBuilder.AddRedisGrainStorage("votes", options => options.ConnectionString = redisAddress);
    }
});

// Add services to the container.
builder.Services.AddServices();
builder.Services.AddEFDbContext(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.AddAppSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(x =>
{
    //x.WithOrigins("http://localhost:8080")
    x.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader();
    //.AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAppSwagger();

app.MapRazorPages();
app.MapAppControllers();

app.Run();
