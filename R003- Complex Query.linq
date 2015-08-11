<Query Kind="Program">
  <NuGetReference Version="3.0.3660">RavenDB.Client</NuGetReference>
  <Namespace>Raven.Abstractions.Indexing</Namespace>
  <Namespace>Raven.Client.Document</Namespace>
  <Namespace>Raven.Client.Indexes</Namespace>
  <Namespace>Raven.Client.Linq</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Raven.Client</Namespace>
</Query>

void Main()
{
	using (var store = new DocumentStore { Url = "http://localhost:8080/", DefaultDatabase = "ddd"})
	{		
		store.Initialize();	
		using (var session = store.OpenSession())
		{
			IndexCreation.CreateIndexes(typeof(Authors_SearchByAnything).Assembly, store);
			
			var query = session.Query<Authors_SearchByAnything.ReduceResult, Authors_SearchByAnything>();
			
			var searches = new[] {
				query.Where(x => x.FullName == "Dan Simmons" || x.BookCount == 2),
				query.Where(x => x.Genres.In("Fantasy","Horror")),			
				query.Search(x => x.Search, "Stephen"),
				query.Search(x => x.Search, "12.34"),
				query.Search(x => x.Search, "Fantasy"),
				query.Search(x => x.Search, "King F*", options: SearchOptions.Or, escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards)
			};
			
			searches.ToList().ForEach(y => y.Select(x => new {x.FullName, x.Genres, x.BookCount, x.YearsPublished, x.Prices}).Dump());
						
		}
	}
}

public class Authors_SearchByAnything : AbstractIndexCreationTask<Author, Authors_SearchByAnything.ReduceResult>
{
	public class ReduceResult
	{
		public string FullName {get; set;}
		public string Genres {get; set;}
		public int BookCount {get; set;}
		public string YearsPublished {get; set;}
		public string Prices {get; set;}
		public string[] Search {get; set;}
	}

	public Authors_SearchByAnything()
	{
		Map = authors => from author in authors		
						 from book in author.Books
						select new ReduceResult() { 
							FullName = author.Name + " " + author.Surname,
							Genres = book.Genre,
							BookCount = 1,
							YearsPublished = book.PublishedOn.Value.Year.ToString(),
							Prices = book.Price.ToString(),
							Search = null
						};
						
		Reduce = reduceResults => from rr in reduceResults
						group rr by rr.FullName into rrFullName
						let newGenres = string.Join(", ", rrFullName.Select(x => x.Genres).Distinct())
						let newBookCount = rrFullName.Sum(x => x.BookCount)
						let newYearsPublished = string.Join(", ", rrFullName.Select(x => x.YearsPublished).Distinct())
						let newPrices = string.Join(", ", rrFullName.Select(x => x.Prices).Distinct())
						select new ReduceResult() { 
							FullName = rrFullName.Key,
							Genres = newGenres,
							BookCount = newBookCount,
							YearsPublished = newYearsPublished,
							Prices = newPrices,
							Search = new string[] {rrFullName.Key, newGenres, newBookCount.ToString(), newYearsPublished, newPrices}
						};						
							
		Stores.Add(x => x.FullName, FieldStorage.Yes);
		Stores.Add(x => x.Genres, FieldStorage.Yes);	
		Stores.Add(x => x.BookCount, FieldStorage.Yes);
		Stores.Add(x => x.YearsPublished, FieldStorage.Yes);	
		Stores.Add(x => x.Prices, FieldStorage.Yes);		
		
		Indexes.Add(x => x.Search, FieldIndexing.Analyzed);
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