import type {
	LogTemplateResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const savedSearches: Array<SavedLogSearchResponseModel> = [
	{
		name: 'Find all logs where the Level is NOT Verbose and NOT Debug',
		query: "Not(@Level='Verbose') and Not(@Level='Debug')",
	},
	{
		name: 'Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)',
		query: 'Has(@Exception)',
	},
	{
		name: "Find all logs that have the property 'Duration'",
		query: 'Has(Duration)',
	},
	{
		name: "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
		query: 'Has(Duration) and Duration > 1000',
	},
	{
		name: "Find all logs that are from the namespace 'Umbraco.Core'",
		query: "StartsWith(SourceContext, 'Umbraco.Core')",
	},
];

export const messageTemplates: LogTemplateResponseModel[] = [
	{
		messageTemplate: 'Create Foreign Key:\n {Sql}',
		count: 90,
	},
	{
		messageTemplate: 'Create Index:\n {Sql}',
		count: 86,
	},
	{
		messageTemplate: 'Create table:\n {Sql}',
		count: 82,
	},
	{
		messageTemplate: 'Create Primary Key:\n {Sql}',
		count: 78,
	},
	{
		messageTemplate: 'Creating data in {TableName}',
		count: 58,
	},
];

export const logLevels = {
	total: 2,
	items: [
		{
			name: 'Global',
			level: 'Information',
		},
		{
			name: 'UmbracoFile',
			level: 'Verbose',
		},
	],
};
