# LiteRepository

There is a lightweight implementation of [Repository Pattern](http://msdn.microsoft.com/en-us/library/ff649690.aspx) based on [Dapper](https://github.com/StackExchange/dapper-dot-net) ORM.

This repository contains 2 components:

* DataRepository: the implementation supports CRUD operations via auto-generated plain SQL (stored procedures haven't been supported yet)
* Connector for SQL Server: implementation of the SQL generator

## Usage

See a sample of using in SqlServerIntegration class

[Install via NuGet](https://www.nuget.org/packages/LiteRepository/)
