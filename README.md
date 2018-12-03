# serilog-enrichers-aspnetcore-httpcontext &middot; [![NuGet](https://img.shields.io/nuget/v/Serilog.Enrichers.AspnetcoreHttpcontext.svg?style=flat-square)](https://www.nuget.org/packages/Serilog.Enrichers.AspnetcoreHttpcontext) [![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/trenoncourt/serilog-enrichers-aspnetcore-httpcontext/blob/master/LICENSE)

> Enriches Serilog events with Aspnetcore HttpContext.  By default logs all information about the Http Request.  Provides access to log whatever you want from the HttpContext, 
including Session, Cookies, Request, Connection, User - basically anything from the HttpContext object. 

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

## The Basics

### Move LoggerConfiguration to the UseSerilog method
The reason for this move is becuase within ASP.NET Core the HttpContext is only available 
after the dependency injection engine is established.  The ``LogggerConfiguration`` setup
is the same as you would do elsewhere, just the location has moved.

### Use the ``WithAspnetcoreHttpcontext`` extension method
Using the method as shown above (with only the ``providers`` parameter passed) will use the default enrichment method.  It will format the [HttpContextCache](./src/Serilog.Enrichers.AspnetcoreHttpcontext/HttpContextCache.cs) object from this 
project, which contains mostly information from the ``Request`` object of the ``HttpContext``.  

### Provide your own method that returns what you want from HttpContext
If you want different items from the ``HttpContext`` in your log entries, you can provide your own method to do this.  A full working example can be seen in the [samples folder](./samples).

You provide a second argument to the ``WithAspnetcoreHttpcontext`` method, which is the name of a method with a signature like this:

```
SomeClassYouDefine WhateverNameYouWant(IHttpConextAccessor hca)
```

``SomeClassYouDefine`` is a simple class that you would create that contains properties for the information from the ``HttpContext`` you want in the log entry.  Then inside the method you "new up" the class into an actual object, and use the ``hca`` parameter's ``HttpContext``
property to fill your object with whatever you want and then return the object.  

Here's the example from the samples folder:
```
...
            .Enrich.WithAspnetcoreHttpcontext(provider,
                            customMethod: CustomEnricherLogic)
...

private static MyObject CustomEnricherLogic(IHttpContextAccessor ctx)
{
    var context = ctx.HttpContext;
    if (context == null) return null;
    
    var myInfo = new MyObject
    {
        Path = context.Request.Path.ToString(),
        Host = context.Request.Host.ToString(),
        Method = context.Request.Method
    };

    var user = context.User;
    if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
    {
        myInfo.UserClaims = user.Claims.Select(a => new KeyValuePair<string, string>(a.Type, a.Value)).ToList();
    }
    return myInfo;
}

public class MyObject
{
    public string Path { get; set; }
    public string Host { get; set; }
    public string Method { get; set; }

    public List<KeyValuePair<string, string>> UserClaims { get; set; }
}

```

### Format (or don't) your log entries
Either the standard ``HttpContextCache`` class or your own custom class will be added to the log entry as the ``HttpContext`` property.  You can use this in your custom formatting logic or just leave it to the built-in formatters to handle.

## Contributing

1. Fork it
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request
