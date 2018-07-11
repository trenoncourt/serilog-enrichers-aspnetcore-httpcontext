# serilog-enrichers-aspnetcore-httpcontext &middot; [![NuGet](https://img.shields.io/nuget/v/Serilog.Enrichers.AspnetcoreHttpcontext.svg?style=flat-square)](https://www.nuget.org/packages/Serilog.Enrichers.AspnetcoreHttpcontext) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/trenoncourt/serilog-enrichers-aspnetcore-httpcontext/blob/master/LICENSE)

> Enriches Serilog events with Aspnetcore HttpContext.

## Installation

```powershell
Install-Package Serilog.Enrichers.AspnetcoreHttpcontext
```

## Usage example

```c#
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((provider, context, loggerConfiguration) =>
                {
                    loggerConfiguration.MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.WithAspnetcoreHttpcontext(provider)
                        .WriteTo.Console(
                            outputTemplate:
                            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{HttpContext}")
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                        {
                            IndexFormat = "serilog-enrichers-aspnetcore-httpcontext-{0:yyyy.MM}"
                        });
                });
```

You'll have enriched log event like pictures listed bellow

![alt text](https://raw.githubusercontent.com/trenoncourt/serilog-enrichers-aspnetcore-httpcontext/master/samples/SerilogAspnetcoreHttpcontextSample/console.png)

![alt text](https://raw.githubusercontent.com/trenoncourt/serilog-enrichers-aspnetcore-httpcontext/master/samples/SerilogAspnetcoreHttpcontextSample/elastic.png)

## Settings

Soon...

## Contributing

1. Fork it
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request
