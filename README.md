# AspNetCore.Identity.DocumentDb

AspNetCore.Identity.DocumentDb is a storage provider for ASP.NET Core Identity that allows you to use Azure DocumentDB
as it's data store instead of the default SQL Server store. It supports all features of Identity, including **full role
support** and **external authentication services**.

## Add AspNetCore.Identity.DocumentDb to your project with NuGet

Run the following command in Package Manager Console:

```shell
Install-Package CodeKoenig.AspNetCore.Identity.DocumentDb -Pre
```

## Supported Identity features

* User Store:
  * Users
  * Claims
  * External Authentication (Logins)
  * Two-Factor-Authentication
  * Roles
  * Passwords
  * Security Stamps
  * Phone Numbers
  * Email
  * Lockout
* Role Store:
  * Roles
  * Role-based Claims

## Quickstart in ASP.NET MVC Core

AspNetCore.Identity.DocumentDb works just like the default SQL Server storage provider:

* When registering services in `ConfigureServices()` in `startup.cs`, you first need to register your `IDocumentClient`
  instance that also AspNetCore.Identity.DocumentDb will resolve to access your DocumentDb database.
* Next, register ASP.NET Identity by calling `services.AddIdentity<DocumentDbIdentityUser, DocumentDbIdentityRole>()`
  as you would with the SQL Server provider, just make sure you specify `DocumentDbIdentityUser` and `DocumentDbIdentityRole`
  as the generic type parameters to use with AspNetIdentity.
* Finally, the actual storage provider can be registered with `.AddDocumentDbStores()`- be sure to configure the options
  for the store and specify at least the `Database` and `UserStoreDocumentCollection` to specify which database
  and document collection AspNetCore.Identity.DocumentDb should use to store data.

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    // Add DocumentDb client singleton instance (it's recommended to use a singleton instance for it)
    services.AddSingleton<IDocumentClient>(new DocumentClient("https://localhost:8081/", "YourAuthorizationKey");

    // Add framework services.
    services.AddIdentity<DocumentDbIdentityUser, DocumentDbIdentityRole>()
        .AddDocumentDbStores(options =>
        {
            options.Database = "YourDocumentDbDatabase";
            options.UserStoreDocumentCollection = "YourDocumentDbCollection";
        })
        .AddDefaultTokenProviders();

    // Further service configurations ...
}
```

> **Important**: AspNetCore.Identity.DocumentDb won't create any database or document collection
> in your DocumentDB. You have to take care that the database and any document collection that you
> want to use with it already exists.

For a complete working sample, look at the sample project in the `/samples` folder in this repository.

## A deeper look

### Storing roles

AspNetCore.Identity.DocumentDB supports roles. If you do not specify a separate collection for the role
store, AspNetCore.Identity.DocumentDB will store roles in the collection that is already used for users.
This is fully supported.

To specify a separate collection as the role store, pass the name of this collection in the `DocumentDbOptions`:

```CSharp
services.AddIdentity<DocumentDbIdentityUser, DocumentDbIdentityRole>()
    .AddDocumentDbStores(options =>
    {
        options.Database = "YourDocumentDbDatabase";
        options.UserStoreDocumentCollection = "YourUsersDocumentDbCollection";
        options.RoleStoreDocumentCollection = "YourRolesDocumentCollection";
    })
```

As with the user store collection and database, also the role collection won't be created by
AspNetCore.Identity.DocumentDB if it doesn't exist. Make sure the collection is created beforehand.

### Storing users and/or roles together with other documents in the same collection

As well as you can store users and roles in the same collection, it is also supported to store
users and roles together with any other document. To be able to distinct users and roles from other
documents, AspNetCore.Identity.DocumentDB stores the type name of the user and role class with the
document in the `documentType` property.

### Automatic partitioning

AspNetCore.Identity.DocumentDB does currently **not support automatic partitioning** in DocumentDB.
Currently you can store users and roles only in a single partition (or in two separate partitions for
users and roles).

Support for automatic partitioning is planned for a future release.

### Indexing

As you need to create the document collections to store users and roles yourself, you are also responsible
for setting up indexes in those document collections. If you go with the default index everything approach,
you're good. If you want to use a more granular indexing approach to save storage and reduce RU cost on
writing new documents, here's a recommendation which properties should be indexed for best possible read
performance:

* User documents:
  * userName
  * normalizedUserName
  * email
  * normalizedEmail
  * logins/
    * loginProvider
    * providerKey
  * roles/
    * roleName
    * normalizedRoleName
  * claims/
    * Type
    * Value
* Role documents:
  * name
  * normalizedName

### Custom user and role classes

You can inherit from `DocumentDbIdentityUser` as well as from `DocumentDbIdentityRole` if you want to
extend those classes. Any additional properties that you provide will be stored in (and also retrieved from)
DocumentDB.

### Restrictions on the ID of a document

There are no restrictions. You can use whatever you see fit. If you don't set an ID for your user or
role document before you store it for the first time, AspNetCore.Identity.DocumentDB will generate a
GUID for the ID automatically, though.

## Tests

This project utilizes a mix of unit and integration tests implemented in xUnit. Integration tests need
a running test instance of DocumentDb to create a temporary test database - it is recommended to use the local
emulator for this, but a test instance in Azure will also work fine.
