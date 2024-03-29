# SwaggerAndHealthCheckBlog

Somewhere to play with new stuff and share it on https://blog.mark-burton.com

## Using HTTP.sys
https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/httpsys?view=aspnetcore-3.1

For this demo the service is running on one http port 1114 which is redirected to the https port 1115 and a management port 1116,
these all need reserving with the netsh urlacl command, here i have used `sddl=D:(A;;GX;;;S-1-1-0)` rather than `user=Everyone` to avoid
issues when the OS language is not English.

``` cmd
netsh http add urlacl url=http://+:1114/ sddl=D:(A;;GX;;;S-1-1-0)
netsh http add urlacl url=https://+:1115/ sddl=D:(A;;GX;;;S-1-1-0)
netsh http add urlacl url=https://+:1116/ sddl=D:(A;;GX;;;S-1-1-0)
```

Optionally create a new self-signed certificate, this could also be done with `dotnet dev-certs https --trust --verbose`
and then get the certhash/thumbprint from cert manager, however for the [netsh http add sslcert command](https://docs.microsoft.com/en-us/windows/win32/http/add-sslcert) the certification has
to be stored in the local computer context and it seems the dotnet command put it in the current user store.

``` ps
PS C:\WINDOWS\system32> New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"       

   PSParentPath: Microsoft.PowerShell.Security\Certificate::LocalMachine\My

Thumbprint                                Subject
----------                                -------
53B0542569C2580C46C34DBE824114D78D4E09FC  CN=localhost
```

Copy this new certification into the `Trusted Root Certification Authorities` node to make the certification trusted in your browser on you local machine.

Now we can use the Thumbprint as the certhash in the netsh sslcert command

Be sure to switch back to cmd to run these commands to avoid an error like `The parameter is incorrect`.

``` cmd
netsh http add sslcert ipport=0.0.0.0:1115 certhash=53B0542569C2580C46C34DBE824114D78D4E09FC appid={8C17E6D4-8534-40F6-B3D3-0296EC69099F}
netsh http add sslcert ipport=0.0.0.0:1116 certhash=53B0542569C2580C46C34DBE824114D78D4E09FC appid={8C17E6D4-8534-40F6-B3D3-0296EC69099F}
```

## Updating an expired certificate
Find the current certification using the following commands.

``` cmd
netsh http show sslcert ipport=0.0.0.0:1115
netsh http show sslcert ipport=0.0.0.0:1116
```

The response will start with the important information including the certhash

``` cmd
SSL Certificate bindings:
-------------------------

    IP:port                      : 0.0.0.0:1115
    Certificate Hash             : 53b0542569c2580c46c34dbe824114d78d4e09fc
    Application ID               : {8c17e6d4-8534-40f6-b3d3-0296ec69099f}
```

Remove the existing binding with the delete command

``` cmd
netsh http delete sslcert ipport=0.0.0.0:1115
```

To confirm the certificate is no longer bound to the ipport run the show command again

``` cmd
netsh http show sslcert ipport=0.0.0.0:1115
```

The output will now show no bindings

``` cmd
SSL Certificate bindings:
-------------------------

The system cannot find the file specified.
```

You can now register a new certificate by following the steps above.

## Nuget package source mapping

Since nuget 6 it is possible to specify which feed certain packages should be restored from.
The rules for this are documented on [Microsoft Learn](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping) and precedence is further discussed in this [Github issue](https://github.com/NuGet/Home/issues/11406)

## References
[zhaytam]: https://blog.zhaytam.com/2020/04/30/health-checks-aspnetcore/
[KeePass.Extensions.Configuration]: https://github.com/adamfisher/KeePass.Extensions.Configuration
[Hanselman HealthChecks]: https://www.hanselman.com/blog/HowToSetUpASPNETCore22HealthChecksWithBeatPulsesAspNetCoreDiagnosticsHealthChecks.aspx
[Displaying ASP.NET Core health checks with Grafana and InfluxDB]: https://gunnarpeipman.com/aspnet-core-health-checks-grafana-influxdb/
