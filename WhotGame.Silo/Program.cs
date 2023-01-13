using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((ctx, orleansBuilder) =>
{
    if (ctx.HostingEnvironment.IsDevelopment())
    {
        // During development time, we don't want to have to deal with
        // storage emulators or other dependencies. Just "Hit F5" to run.
        orleansBuilder
            .UseLocalhostClustering(gatewayPort: 30000)
            .AddMemoryGrainStorage("whot");
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
builder.Services.AddRazorPages();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
