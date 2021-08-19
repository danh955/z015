# Entities

The database entities are managed in the Z015.Repository name space.

### Code first entity framework

- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core)
- [DbContext Lifetime, Configuration, and Initialization](https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration)
- [Design-time DbContext Creation](https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation)
 
### Migration

To create the migration for the first time, do the Add-Migration.

- [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools)
- **Add-Migration** *name*
  - Creates a new migration class as per specified name with the Up() and Down() methods,
- **Update-Database**
  - Executes the last migration file created by the Add-Migration command and applies changes to the database schema.

### Add to your startup or main program

```csharp
.ConfigureServices((hostContext, services) =>
{
    var connectionString = this.Configuration.GetConnectionString("SqlDatabase");
    services.AddDbContextPool<RepositoryDbContext>(
        options => options.UseSqlServer(
                            connectionString,
                            p => p.MigrationsAssembly(typeof(Startup).Namespace)));
}
```
Or the factory method. In dependency injection, use the IDbContextFactory interface.
```csharp
.ConfigureServices((hostContext, services) =>
{
    var connectionString = this.Configuration.GetConnectionString("SqlDatabase");
    services.AddPooledDbContextFactory<RepositoryDbContext>(
        options => options.UseSqlServer(
                            connectionString,
                            p => p.MigrationsAssembly(typeof(Startup).Namespace)));
}
```
The p.MigrationsAssembly(name) will put the migration in the project where this is declared.
Replace "UseSqlServer" with the relational database of your choice.

Note: Entity Framework is a repository.
