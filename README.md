# LiteRepository

LiteRepository is:

* Generic Repository class (supports only CRUD operations)
* POCO mapper (one class -> one database table by using attributes, if necessary)
* Toolkit for building CRUD queries without writing SQL
* The ability to create own repositories by using toolkit and POCO mapper
* The ability to add new RDBMS

Designed for use with plain SQL. Support for Stored Procedures is not implemented and not planned (use pure Dapper instead).

[Install via NuGet](https://www.nuget.org/packages/LiteRepository/)

**Current status is an early alpha. Use on own risk.** About all issues please report in the tracker.

## Usage

Each RDBMS includes a specialized dialect provider. At this moment LiteRepository supports only Sql Server (included in standart package).

To congigure LiteRepository you need to create Db object:

	var dialect = new SqlServerDialect();
	var dbConnectionFactory = () => new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString);
	var db = new Db(dialect, dbConnectionFactory);


The first parameter is the provider of the dialect. The second parameter is factory for database connection (or single connection).

Db is responsible for connection with the database and providing tools for querying the database.

### Repository

For using generic Repository you need to create Repository object by passing Db object as constructor's parameter:

	var repository = new Repository<Entity, EntityKey>(db);

The first type parameter - is the type of data entity. The second - is the type of the entity's key. The entity must be inherited from it's key type.

For inserting the entity into database call InsertAsync method (all methods of Repository class are async):

	var entity = new Entity { FirstName = "Alex", SecondName = "Lion" };
	await repository.InsertAsync(entity);

The method returns the added entity. In case of simple entity - it's the same entity which was passed as parameter. In case of identity entity (must implement IIdentityEntity interface) it returns result of IIdentityEntity.UpdateId entity's method.

For updating the entity call UpdateAsync:

	var entity = new Entity { FirstName = "Alex", SecondName = "Lion" };
	await repository.UpdateAsync(entity);

The method returns a number of affected rows. 1 if data was successfully updated and 0 if entity was not found.

For deleting the entity call DeleteAsync:

	var key = new EntityKey { Id = 12 };
	await repository.DeleteAsync(entity);

The method returns a number of affected rows. 1 if data was successfully deleted and 0 if entity with this key was not found.

For getting the entity by it's key call GetAsync:

	var key = new EntityKey { Id = 12 };
	var entity = await repository.GetAsync(entity);

The method returns entity or null, if entity with this key was not found.

For getting all entities from database call GetAllAsync:

	var entities = await repository.GetAllAsync(entity);

The method returns a collection of enitities (empty if no entity was found).

For getting a count of entities in database call GetCountAsync:

	var entities = await repository.GetAllAsync(entity);

The method returns a count of all enitities (actually, it's a query - SELECT COUNT(1) FROM table).

### POCO mapper

One class is mapped in one table in database. By default, the name of class = the name of table and property's name = the name of the table column. To change this behaviour there are three attributes:

* SqlAlias. Allows to set database name of class or property.
* SqlIgnore. Allows to hide property from mapper.
* SqlKey. Allows to mark property as a part of the primary key.

For example, this class:

	[SqlAlias("students")]
	public class Student
	{
		[SqlKey]
		public Id { get; set; }
		[SqlAlias("first_name")]
		public FirstName { get; set; }
		[SqlAlias("second_name")
		public SecondName { get; set; }
		[SqlIgnore]
		public DateTime CacheValidateTime { get; set; }
	}

corresponds to a table in the database:

	create table students (
		Id,
		first_name,
		second_name
		);

> The data type has been specifically omitted, as the possibility of creating a table based on the POCO is not provided.

For tables with identities Entity must implement IIdentityEntity interface:

    public interface IIdentityEntity
    {
        long Id { get; }
        object UpdateId(long id);
    }

In  this case, all SqlKey attributes will be ignored. Such entity's primary key consists of one field - Id. The UpdateId method will be invoked when a record is added to the database. The generated Id will be passed as the parameter. It is expected that the method will return the entity with the changed Id

### Toolkit

Toolkit is implemented in the Db class. This is a set of methods that simplifies the implementation of CRUD queries. Unlike the repository, each method has both synchronous and asynchronous version.

To execute any code within the same open connection is used by the Exec method:

	var execResult = db.Exec(connection =>
	{
		return connection.Execute("select * from table");
	});

All the methods of Db class are made through this method.

To add a record to the database use the Insert method:

	var execResult = db.Insert(new Entity { Id = 12, FirstName = "Alex", SecondName = "Lion" });

SQL:
	
	INSERT INTO students (id, first_name, second_name)
	VALUES (@Id, @FirstName, @SecondName);

In the case of a table with identity:

	INSERT INTO students (first_name, second_name)
	VALUES (@FirstName, @SecondName)
	SELECT SCOPE_IDENTITY()

To update the record you can use one of two methods. The first updates the record as a whole, finding it by the primary key:

	var entity = new Entity { Id = 42, FirstName = "Alex", SecondName = "Lion" };
	var execResult = db.Update(entity);

SQL:
	
	UPDATE students SET
		first_name = @FirstName
		second_name = @SecondName
	WHERE id = @Id;

The second allows you to update only selected fields for multiple records that will satisfy the condition:

	var execResult = db.Update<Entity>(new { FirstName = "Alexander" }, i => i.FirstName = "Alex");

SQL:
	
	UPDATE students SET first_name = @FirstName WHERE first_name = 'Alex';

> The use of parameters in condition is not allowed

To delete records you can use one of three methods. 
To delete a single record by key, use the method:

	var execResult = db.Delete<Entity, EntityKey>(new EntityKey(42));

SQL:
	
	DELETE FROM students WHERE id = @Id;

To delete multiple records satisfying the condition use the method:

	var execResult = db.Delete<Entity>(i => i.FirstName.StartsWith("Al"));

SQL:
	
	DELETE FROM students WHERE first_name like 'Al%';

You can also use parameters in the query:

	var param = new { FirstName = "Alex" };
	var execResult = db.Delete<Entity>(i => i.FirstName = param.FirstName, param);

SQL:
	
	DELETE FROM students WHERE first_name = @FirstName;

To delete all records use Truncate method:

	db.Truncate<Entity>();

SQL:
	
	TRUNCATE TABLE students;

To receive data use one of three methods. To retrieve a record by primary key the GetByKey method is used. You can specify to receive only a subset of fields of the original recording. 

	var p = new { FirstName = "", SecondName = "" };
	var key = new EntityKey { Id = 42 };
	var count = db.GetByKey<Entity>(key, p.GetType());

SQL:
	
	SELECT first_name, second_name FROM students WHERE id = @Id

To retrieve a set of records that meet specific criteria use the Get method. You can set to retrieve only some fields and set the sort order:

	var p = new { FirstName = "", SecondName = "" };
	var param = new { FirstName = "Alex" };
	var count = db.Get<Entity>(p.GetType(), where: i => i.FirstName = param.FirstName, param, orderBy: i => i.OrderBy(x => x.SecondName));

SQL:
	
	SELECT first_name, second_name FROM students WHERE first_name = @FirstName ORDER BY second_name

To obtain the scalar expression use GetScalar method. It supports only Count, Average, Sum:

	var param = new { FirstName = "Alex" };
	var count = db.GetScalar<long>(i => i.Count, where: i => i.FirstName = param.FirstName, param);

SQL:
	
	SELECT COUNT(1) FROM students WHERE first_name == @FirstName;

**The syntax of conditions**

The condition is given by the equation. Supports binary comparison operations (!=, <, >, <=, >=, ==), binary || and &&, and unary !. For string fields methods StartWith, EndsWith, Contains, ToLower and ToUpper are supported. For fields of DateTime type a constant can be set through the constructor.

**The syntax of order**

The chain of methods OrderBy and OrderByDescending is supported.
