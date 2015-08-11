<Query Kind="Program">
  <NuGetReference Version="3.0.3660">RavenDB.Client</NuGetReference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Raven.Client.Document</Namespace>
  <Namespace>Raven.Client.Indexes</Namespace>
  <Namespace>Raven.Abstractions.Indexing</Namespace>
  <Namespace>Raven.Abstractions.Data</Namespace>
</Query>

void Main()
{
	using (var store = new DocumentStore { Url = "http://localhost:8080/", DefaultDatabase = "ddd"})
	{
		store.Initialize();	
		using (var session = store.OpenSession())
		{		
			IndexCreation.CreateIndexes(typeof(Books_All).Assembly, store);
		
			"".Dump("I am adding 5000 documents and naively expecting to get them back");
			for(var i = 0; i < 5000; i++)
				session.Store(new Book());
			session.SaveChanges();
			
			session.Query<Book, Books_All>()				
				.ToList().Count().Dump("Get results immediately");
				
			Thread.Sleep(2000);
			session.Query<Book, Books_All>()
				.ToList().Count().Dump("OK, then wait a few seconds and get results");
			
			session.Query<Book, Books_All>()				
				.Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
				.ToList().Count().Dump("In this case wait until index update finishes");				
						
			session.Query<Book, Books_All>()				
				.Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
				.Take(int.MaxValue)
				.ToList().Count().Dump("Ah, please, give me ALL my data!");
				
			var allRecords = 0;
			using (var enumerator = session.Advanced.Stream(session.Query<Book, Books_All>()))
				while (enumerator.MoveNext())
					allRecords++;
			allRecords.Dump("PLEASE! GIVE ME MY DATA!");
				
			session.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName",
				new IndexQuery { Query = "Tag:Books"},
				new BulkOperationOptions {AllowStale = false}
			);				
		}
	}
}

public class Books_All : AbstractIndexCreationTask<Book, Books_All.ReduceResult>
{
	public class ReduceResult
	{
		public string Title {get; set;}
		public string Genre {get; set;}
	}

	public Books_All()
	{
		Map = books => from book in books
						select new Book {
							Title = book.Title,
							Genre = book.Genre
						};
							
		Stores.Add(x => x.Title, FieldStorage.Yes);
		Stores.Add(x => x.Genre, FieldStorage.Yes);	
	}
}

// Define other methods and classes here
public class Author {
	public string Name { get; set; }
	public string Surname { get; set; }
	public Book[] Books { get; set; }
}
public class Book {
	public string Title { get; set; }
	public string Genre { get; set; }
	public DateTime? PublishedOn { get; set; }
	public decimal? Price { get; set; }
}