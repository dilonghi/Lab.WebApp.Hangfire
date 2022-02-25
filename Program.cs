using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHangfire(configuration => configuration
                .UseRecommendedSerializerSettings()
                .UseSQLiteStorage());

// Define a quantidade de retentativas aplicadas a um job com falha.
// Por padrão serão 10, aqui estamos abaixando para duas com intervalo de 5 minutos.
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
{
    Attempts = 3,
    DelaysInSeconds = new int[] { 300 }
});

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard();



BackgroundJob.Schedule(
    () => Console.WriteLine("Job Delayed: 2 minutos após o início da aplicação"),
    TimeSpan.FromMinutes(2));

RecurringJob.AddOrUpdate(
    "Meu job recorrente",
    () => Console.WriteLine((new Random().Next(1, 200) % 2 == 0)
        ? "Job recorrente gerou um número par"
        : "Job recorrente gerou um número ímpar"),
    Cron.Minutely,
    TimeZoneInfo.Local);


var jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Job fire-and-forget pai!"));
BackgroundJob.ContinueJobWith(
    jobId,
    () => Console.WriteLine($"Job continuation! (Job pai: {jobId})"));


app.Run();
