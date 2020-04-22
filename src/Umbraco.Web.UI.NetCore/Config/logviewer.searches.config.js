[
  {
    "name": "Find all logs where the Level is NOT Verbose and NOT Debug",
    "query": "Not(@Level='Verbose') and Not(@Level='Debug')"
  },
  {
    "name": "Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)",
    "query": "Has(@Exception)"
  },
  {
    "name": "Find all logs that have the property 'Duration'",
    "query": "Has(Duration)"
  },
  {
    "name": "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
    "query": "Has(Duration) and Duration > 1000"
  },
  {
    "name": "Find all logs that are from the namespace 'Umbraco.Core'",
    "query": "StartsWith(SourceContext, 'Umbraco.Core')"
  },
  {
    "name": "Find all logs that use a specific log message template",
    "query": "@MessageTemplate = '[Timing {TimingId}] {EndMessage} ({TimingDuration}ms)'"
  },
  {
    "name": "Find logs where one of the items in the SortedComponentTypes property array is equal to",
    "query": "SortedComponentTypes[?] = 'Umbraco.Web.Search.ExamineComponent'"
  },
  {
    "name": "Find logs where one of the items in the SortedComponentTypes property array contains",
    "query": "Contains(SortedComponentTypes[?], 'DatabaseServer')"
  },
  {
    "name": "Find all logs that the message has localhost in it with SQL like",
    "query": "@Message like '%localhost%'"
  },
  {
    "name": "Find all logs that the message that starts with 'end' in it with SQL like",
    "query": "@Message like 'end%'"
  }
]
