# LiteRepository

LiteRepository it is:

* Generic Repository class (supports only CRUD operations)
* POCO mapper (one class -> one database table by using attributes, if nessesary)
* Toolkit for building CRUD queries without writing SQL
* The ability to create own repositories by using toolkit and POCO mapper
* The ability to add new RDBMS

Designed for use with plain SQL. Support for Stored Procedures is not implemented and not planned (use pure Dapper instead).

[Install via NuGet](https://www.nuget.org/packages/LiteRepository/)

## Usage

Each RDBMS includes a specialized dialect provider. At this moment supports only Sql Server (incuded in standart package).

To congigure LiteRepository you need to create Db object:

	var dialect = new SqlServerDialect();
	var dbConnectionFactory = () => new SqlConnection(ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString);
	var db = new Db(dialect, dbConnectionFactory);


The first parameter is passed to the provider dialect. The second parameter is passes factory for database connection (or single connection).

The responsibilities of the Db is connection with the database and providing tools for querying the database.

### Repository

For using generic Repository you need to create Repository object by passing Db object as constructors parameter:

	var repository = new Repository<Entity, EntityKey>(db);

The first type parameter - is the type of data entity. The second - the type of the entities key. Entity must be inherited from its key type.

For insert entity into database call InsertAsync method (all methods of Repository class is async):

	var entity = new Entity { FirstName = "Alex", SecondName = "Lion" };
	await repository.InsertAsync(entity);

Method returns added entity. In case of simple entity - it's the same entity witch was passed as parameter. In case of identity entity (must implement IIdentityEntity interface) it returns result of IIdentityEntity.UpdateId(long id) entity's method.

For update entity call UpdateAsync:

	var entity = new Entity { FirstName = "Alex", SecondName = "Lion" };
	await repository.UpdateAsync(entity);

Method returns number of affected rows. 1 if data was successfull updated and 0 if entity was not found.

For delete entity call DeleteAsync:

	var key = new EntityKey { Id = 12 };
	await repository.DeleteAsync(entity);

Method returns number of affected rows. 1 if data was successfull deleted and 0 if entity with this key was not found.

For get entity by its key call GetAsync:

	var key = new EntityKey { Id = 12 };
	var entity = await repository.GetAsync(entity);

Method returns entity or null, if entity with this key was not found.

For get all entities from database call GetAllAsync:

	var entities = await repository.GetAllAsync(entity);

Method returns collection of enitities (empty if entity with this key was not found).

For get count of entities in database call GetCountAsync:

	var entities = await repository.GetAllAsync(entity);

Method returns count of all enitities (actually, it's a query - SELECT COUNT(1) FROM table).

### POCO mapper

One class mapped into one table in database. By default, class' name = name of table and property's name = name of the table column. To change this behaviour there are three attributes:

* SqlAlias. Allows to set database name of class or property.
* SqlIgnore. Allows to hide property from mapper.
* SqlKey. Allows to mark property as a part of the primary key.

For example, this class' description:

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

In  this case, all SqlKey attributes will be ignored. Such entity's primary key consists of one field - Id. The UpdateId method will be invoked when a record is added to the database. The parameter passed will be assigned to the entry Id. It is expected that the method will return the entity with the changed Id

### Toolkit

Toolkit is implemented in the Db class. This is a set of methods that simplify the implementation of CRUD queries. Unlike the repository, each method has both synchronous and asynchronous version.

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

To update the record you can use one of two methods. The first updates the record as a whole, finding it by primary key:

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
To delete a single record by key, use the metod Delete<E, K>(key):

	var execResult = db.Delete<Entity, EntityKey>(new EntityKey(42));

SQL:
	
	DELETE FROM students WHERE id = @Id;

To delete multiple records satisfying the condition used by the method Delete<E>(Expression, param):

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

To receive data using one of three methods. To retrieve a record by primary key the Get method is used. You can specify to receive only a subset of fields of the original recording. 

	var p = new { FirstName = "", SecondName = "" };
	var key = new EntityKey { Id = 42 };
	var count = db.Get<Entity>(key, p.GetType());

SQL:
	
	SELECT first_name, second_name FROM students WHERE id = @Id

To retrieve a set of records that meet specific criteria use the Get method. You can set retrieve only some fields and set the sort order:

	var p = new { FirstName = "", SecondName = "" };
	var param = new { FirstName = "Alex" };
	var count = db.Get<Entity>(p.GetType(), where: i => i.FirstName = param.FirstName, param, orderBy: i => i.OrderBy(x => x.SecondName));

SQL:
	
	SELECT first_name, second_name FROM students WHERE first_name = @FirstName ORDER BY second_name

To obtain the scalar expression is used GetScalar method. Only supports Count, Average, Sum:

	var param = new { FirstName = "Alex" };
	var count = db.GetScalar<long>(i => i.Count, where: i => i.FirstName = param.FirstName, param);

SQL:
	
	SELECT COUNT(1) FROM students WHERE first_name == @FirstName;

**The syntax of conditions**

The condition is given by the equation. Supports binary comparison operations (!=, <, >, <=, >=, ==), binary || and &&, and unary !. For string fields are supported methods StartWith, EndsWith, Contains, ToLower and ToUpper. For fields of type DateTime constant can be set through the constructor.

**The syntax of order**

Supported the chain of methods OrderBy and OrderByDescending.