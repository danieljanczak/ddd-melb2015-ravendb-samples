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
			session.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName",
				new IndexQuery { Query = "Tag:Authors"},
				new BulkOperationOptions {AllowStale = false}
			);	
			session.Advanced.DocumentStore.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName",
				new IndexQuery { Query = "Tag:Books"},
				new BulkOperationOptions {AllowStale = false}
			);	
			foreach (var index in session.Advanced.DocumentStore.DatabaseCommands.GetIndexes(0,100).ToList())
				if (index.Name != "Raven/DocumentsByEntityName")
					session.Advanced.DocumentStore.DatabaseCommands.DeleteIndex(index.Name);
				
		}
	}
}
