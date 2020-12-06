## Tools
https://docs.microsoft.com/en-us/ef/core/cli/dotnet

### Reverse enginer the database
Be aware the parameters in these commands like --table are case sensitive `language`
dotnet tool update --global dotnet-ef
PS C:\source\repos\MyApp> cd .\src\MyApp.Lib\
dotnet ef dbcontext scaffold "Data Source=.\MSSQLSERVER2017;Database=MyDB;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer --context TranslationContext --table sys_translation --table language
Use options like --context and --namespace to generate some temp classes for comparison without needed to use --force to overwrite
dotnet ef dbcontext scaffold "Data Source=.\MSSQLSERVER2017;Database=MyDB;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer --context temp --table sys_translation --table language --context temp --namespace temp --force

## Samples used
The Generic host brings all the defaults of asp.net, avoiding the boiler plate code of setting up configuration, DI, logging etc.

For the command line args parsing and help generation i used the new System.CommandLine and DragonFruit
https://github.com/dotnet/command-line-api/blob/main/samples/HostingPlayground/Program.cs

## Similar code
look at the translation service for numeric to boolean converter to use in the EF datacontext, this should be a shared DAL.
https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions
use bool to numeric converter

## User Secrets
`dotnet user-secrets init` has been executed on this project which added `<UserSecretsId>2cdc6706-e858-49f8-b4aa-5c5187ca6f6b</UserSecretsId>` to the TranslationConsole.csproj file.
To set the connection string locally run the following command.
`dotnet user-secrets set ConnectionStrings:MyDB "Data Source=.\MSSQLSERVER2017;Initial Catalog=MyDB;Integrated Security=true;"`
