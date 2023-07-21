using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => new OkResult());

var computeLocation = "AWS App Runner";

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
{
    computeLocation = "AWS Lambda";
}

app.MapGet("/compute", () => new OkObjectResult(new
{
    message = $"Hello From {computeLocation}"
}));

app.MapControllers();

app.Run();
