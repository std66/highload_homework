using System.Net;
using System.Text.Json;
using TomiSoft.HighLoad.App;
using TomiSoft.HighLoad.App.Models.Api;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddLogging()
    .ConfigureHttpJsonOptions(options => {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    });

var app = builder.Build();

app.Use(async (context, next) => {
    try {
        await next(context);
    }
    catch (Exception e) {
        context.Response.StatusCode = e switch {
            BadHttpRequestException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ErrorResponse(e.Message), AppJsonSerializerContext.Default.ErrorResponse)
        );
    }
});

app.RegisterEndpoints();

app.Run();
