import { LogLevelModel, LogMessageModel } from '@umbraco-cms/backend-api';

const allLogs = [
	{
		timestamp: '2023-02-14T12:02:15.8094382+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c80ec31d-3d58-401a-9361-1ae7451c215f',
			},
			{
				name: 'HttpRequestNumber',
				value: '982',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:15.7464278+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '77052e32-93ab-426f-b96c-4c71fab7d48a',
			},
			{
				name: 'HttpRequestNumber',
				value: '981',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:10.7972442+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '1cec3ec5-4980-4bfc-bdf4-085070da6f4d',
			},
			{
				name: 'HttpRequestNumber',
				value: '980',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:10.735678+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '6941710a-4f09-41b2-8f11-fce2c813ad10',
			},
			{
				name: 'HttpRequestNumber',
				value: '979',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:05.7944643+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '5e129d0e-595d-43f4-9055-7ea218347659',
			},
			{
				name: 'HttpRequestNumber',
				value: '978',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:05.7340093+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'a760748a-43cd-4132-9f51-c518df8ae368',
			},
			{
				name: 'HttpRequestNumber',
				value: '977',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:00.7904684+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '3acbe275-1340-4d92-a2f9-8b15eea7c5da',
			},
			{
				name: 'HttpRequestNumber',
				value: '976',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:02:00.7298561+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '09ab7c8b-ae78-4b86-adff-e82bf9bed66e',
			},
			{
				name: 'HttpRequestNumber',
				value: '975',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:55.7815654+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '3252af15-c998-4463-b9c0-18d663d65c53',
			},
			{
				name: 'HttpRequestNumber',
				value: '974',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:55.7215251+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'aa6552dc-226d-4302-ab2f-6a1ae7b82205',
			},
			{
				name: 'HttpRequestNumber',
				value: '973',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:50.7812088+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '85f8e869-9b32-49f7-a0b6-e1be33258230',
			},
			{
				name: 'HttpRequestNumber',
				value: '972',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:50.7176788+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '70effdd7-f20a-4c54-bdf1-573f12c5c4df',
			},
			{
				name: 'HttpRequestNumber',
				value: '971',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:45.7721389+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '62c6c02d-91b2-414d-82d6-ba6485cd901d',
			},
			{
				name: 'HttpRequestNumber',
				value: '970',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:45.7101655+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '88ee8366-0cbe-465f-ab06-39e082f62cc3',
			},
			{
				name: 'HttpRequestNumber',
				value: '969',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:40.7656432+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '72c6403d-097d-4274-b77e-9c33afe05705',
			},
			{
				name: 'HttpRequestNumber',
				value: '968',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:40.7034373+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'da7ddb6d-ec9b-44aa-9376-b38f9dbf16ea',
			},
			{
				name: 'HttpRequestNumber',
				value: '967',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:35.7586458+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'ada57301-d4fe-4399-acab-5506b96f0724',
			},
			{
				name: 'HttpRequestNumber',
				value: '966',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:35.6957643+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '4603522a-12fa-43fa-b171-170c0a7dee91',
			},
			{
				name: 'HttpRequestNumber',
				value: '965',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:30.7559798+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'bb012edc-36c7-4426-86e3-525e8c2ff44e',
			},
			{
				name: 'HttpRequestNumber',
				value: '964',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:30.6937419+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '1180c2e4-ede3-458a-97ca-c733b4224718',
			},
			{
				name: 'HttpRequestNumber',
				value: '963',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:25.744449+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '7c5d8901-3453-402d-881c-12d3c4d9477a',
			},
			{
				name: 'HttpRequestNumber',
				value: '962',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:25.6824877+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'a2ae4602-2a58-4521-99dd-6fa41562b004',
			},
			{
				name: 'HttpRequestNumber',
				value: '961',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:20.7317217+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '0b55b0c8-57f3-4ed9-ae8e-1cb85cb11077',
			},
			{
				name: 'HttpRequestNumber',
				value: '960',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:20.6696118+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c502a5a8-c8cb-4cec-ac6c-f9b66b3bad33',
			},
			{
				name: 'HttpRequestNumber',
				value: '959',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:15.724729+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'bcada94f-9851-4c81-8da0-1bf27be66573',
			},
			{
				name: 'HttpRequestNumber',
				value: '958',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:15.662463+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f761bc23-9e7f-45e7-8847-7885624dd261',
			},
			{
				name: 'HttpRequestNumber',
				value: '957',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:10.7235889+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '88785b30-85c1-40da-bba9-0663c5b38817',
			},
			{
				name: 'HttpRequestNumber',
				value: '956',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:10.6605318+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'a26b64b4-5b83-405d-9860-e01bcbc0edaa',
			},
			{
				name: 'HttpRequestNumber',
				value: '955',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:05.7157154+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'b192ac30-88f0-476d-a9ab-6462a4ca5195',
			},
			{
				name: 'HttpRequestNumber',
				value: '954',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:05.6547824+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '97ee6cef-af1e-472a-acf0-d296890a1916',
			},
			{
				name: 'HttpRequestNumber',
				value: '953',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:00.7039952+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'b537e0e1-e4ed-4f6b-afce-11c19a798632',
			},
			{
				name: 'HttpRequestNumber',
				value: '952',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:01:00.6417516+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '3011bb2a-e3df-4dc6-a46d-9619287ad9bb',
			},
			{
				name: 'HttpRequestNumber',
				value: '951',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:55.6948223+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f43990a1-fdeb-4169-85c1-60b6833b4c82',
			},
			{
				name: 'HttpRequestNumber',
				value: '950',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:55.6326808+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '43def4b0-c9e6-41db-9031-f7c3fc112900',
			},
			{
				name: 'HttpRequestNumber',
				value: '949',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:50.6948589+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'e048057f-5dc0-408f-a560-2044b99e53a3',
			},
			{
				name: 'HttpRequestNumber',
				value: '948',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:50.632697+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'ff426368-f6e5-4e9d-a2d2-0d061bedb688',
			},
			{
				name: 'HttpRequestNumber',
				value: '947',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:45.6943493+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '0f2f6a91-487b-440d-9350-9737bed00c8d',
			},
			{
				name: 'HttpRequestNumber',
				value: '946',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:45.6319218+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'fc244d40-46ae-49f9-8eee-ef5e41738283',
			},
			{
				name: 'HttpRequestNumber',
				value: '945',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:40.6902419+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '5d3e350c-0b62-4122-9850-1b821fc14824',
			},
			{
				name: 'HttpRequestNumber',
				value: '944',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:40.6274587+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f4101feb-d20b-4007-b0d8-5fb8448b0ba5',
			},
			{
				name: 'HttpRequestNumber',
				value: '943',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:35.6787856+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '136c6fa2-59b3-4a35-bf62-41b4b7e47aae',
			},
			{
				name: 'HttpRequestNumber',
				value: '942',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:35.6168559+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'fd9d8b63-8cbe-448d-acfe-fb0225630c8f',
			},
			{
				name: 'HttpRequestNumber',
				value: '941',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:30.6723677+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '5f550460-063c-4ffe-b4b7-977916a0bf3c',
			},
			{
				name: 'HttpRequestNumber',
				value: '940',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:30.6121168+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '7205a37f-3c57-4388-86d0-aa82334ac8b9',
			},
			{
				name: 'HttpRequestNumber',
				value: '939',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:25.6705459+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '11c619df-ec09-400d-bac3-0796ec8313b3',
			},
			{
				name: 'HttpRequestNumber',
				value: '938',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:25.6072437+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '4acb6550-415f-4acd-9d5d-d16e3f24358d',
			},
			{
				name: 'HttpRequestNumber',
				value: '937',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:20.655909+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '538ddeb5-5f61-4f0f-b997-d730ca43c308',
			},
			{
				name: 'HttpRequestNumber',
				value: '936',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:20.593934+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '0bbed3f1-3732-4961-987b-dee855bf14e5',
			},
			{
				name: 'HttpRequestNumber',
				value: '935',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:15.652173+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '2fc31793-1567-4166-ba1d-fedc5aa86c76',
			},
			{
				name: 'HttpRequestNumber',
				value: '934',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:15.5893967+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'a1229318-a1f3-45ae-81ea-553c6380dc63',
			},
			{
				name: 'HttpRequestNumber',
				value: '933',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:10.638341+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '3c73f5d8-fb91-4cb1-99ea-710dc5a077d0',
			},
			{
				name: 'HttpRequestNumber',
				value: '932',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:10.5772254+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c90cfc2c-4257-4963-b983-66e8154d411b',
			},
			{
				name: 'HttpRequestNumber',
				value: '931',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:05.6357729+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '1545fb5a-eb3f-42ad-b996-83a25cc2884f',
			},
			{
				name: 'HttpRequestNumber',
				value: '930',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:05.5733282+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c9d266d1-4934-46c7-9b53-ed09fbd886a0',
			},
			{
				name: 'HttpRequestNumber',
				value: '929',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:00.6298079+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'de2e3225-9d89-4a80-9ee5-e71690355f9d',
			},
			{
				name: 'HttpRequestNumber',
				value: '928',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T12:00:00.5660574+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '82db2316-2b67-451c-86c0-dd45244c1d96',
			},
			{
				name: 'HttpRequestNumber',
				value: '927',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:55.6191475+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '8dc85495-24f2-4842-8ff5-d0374cad3741',
			},
			{
				name: 'HttpRequestNumber',
				value: '926',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:55.5568848+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'cc428ec4-db17-4cee-981d-901fde3af15c',
			},
			{
				name: 'HttpRequestNumber',
				value: '925',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:50.6177966+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '245be706-df80-4501-a915-d2af6e44af26',
			},
			{
				name: 'HttpRequestNumber',
				value: '924',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:50.5567918+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '8d5082a3-4ca1-4fb2-976b-8a761a245661',
			},
			{
				name: 'HttpRequestNumber',
				value: '923',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:45.6058749+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '8fd1dee6-eb15-4081-b623-f4139cc465fd',
			},
			{
				name: 'HttpRequestNumber',
				value: '922',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:45.5419858+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '87434476-9f86-49f5-bacd-37978f97ec3a',
			},
			{
				name: 'HttpRequestNumber',
				value: '921',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:40.6030991+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '3a79d410-232f-46f0-8521-f71c58cefcd3',
			},
			{
				name: 'HttpRequestNumber',
				value: '920',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:40.5414694+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '0fda8a3d-b7df-46e4-bf3d-0cf15829f6a7',
			},
			{
				name: 'HttpRequestNumber',
				value: '919',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:35.5924386+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '54f6057e-d0f6-46d0-b7b4-082f315c4533',
			},
			{
				name: 'HttpRequestNumber',
				value: '918',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:35.5304962+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '39ae83f8-0778-474a-b41f-eea93b82b201',
			},
			{
				name: 'HttpRequestNumber',
				value: '917',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:30.5881163+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c521cadd-3e47-4eab-b640-ea9dfc50abb8',
			},
			{
				name: 'HttpRequestNumber',
				value: '916',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:30.5260781+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '5c3f597f-abcd-41d5-9360-8bd725c85b60',
			},
			{
				name: 'HttpRequestNumber',
				value: '915',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:25.5753438+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '886f9acd-60bd-4ed4-8cf2-1c35b007c58e',
			},
			{
				name: 'HttpRequestNumber',
				value: '914',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:25.5127958+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c215f085-f3b8-4150-b77c-0a9882404530',
			},
			{
				name: 'HttpRequestNumber',
				value: '913',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:20.571759+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'b3d613ba-6a53-4d6e-9b1a-c755752fe566',
			},
			{
				name: 'HttpRequestNumber',
				value: '912',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:20.5091665+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f97c2d30-f658-447e-b728-136bcd824bc9',
			},
			{
				name: 'HttpRequestNumber',
				value: '911',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:15.569587+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f7d59956-1034-4698-aad1-c2da66d067e8',
			},
			{
				name: 'HttpRequestNumber',
				value: '910',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:15.5076538+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'a98bb970-a9f1-4a31-b22c-6874e7bcc811',
			},
			{
				name: 'HttpRequestNumber',
				value: '909',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:10.5556531+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '188b901c-9022-4e8e-80c7-45bb8d5c7c0a',
			},
			{
				name: 'HttpRequestNumber',
				value: '908',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:10.5074665+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'dba6dbdd-2142-4803-8223-cce07f231a51',
			},
			{
				name: 'HttpRequestNumber',
				value: '907',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:05.5524744+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '07d65eca-a9ca-4109-8ad8-4db2a737cc68',
			},
			{
				name: 'HttpRequestNumber',
				value: '906',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:05.5045339+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'c75e74d2-dec9-4525-8966-34e455047231',
			},
			{
				name: 'HttpRequestNumber',
				value: '905',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:00.5441587+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '4787c7ab-a877-47e5-96fa-a8cdbc8eb427',
			},
			{
				name: 'HttpRequestNumber',
				value: '904',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:59:00.4976461+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '6fd66545-4305-4f30-9791-54be1d151483',
			},
			{
				name: 'HttpRequestNumber',
				value: '903',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:55.5306481+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '06ac6e6f-d866-48e7-ae49-b908cd472555',
			},
			{
				name: 'HttpRequestNumber',
				value: '902',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:55.4849332+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'a8a3191e-1324-49b7-9fc5-c65b99f29c37',
			},
			{
				name: 'HttpRequestNumber',
				value: '901',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:50.5183018+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '5c3c404b-03bb-47ee-8e60-2678249015a8',
			},
			{
				name: 'HttpRequestNumber',
				value: '900',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:50.4715437+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '40090ff4-ac41-4e2d-a69b-99414ce77df5',
			},
			{
				name: 'HttpRequestNumber',
				value: '899',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:45.5057522+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'b4cf7b6b-ebaf-40d0-97d6-69acd93bec86',
			},
			{
				name: 'HttpRequestNumber',
				value: '898',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:45.4595094+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '17ee778c-62bb-4025-a232-bfaf76eda60b',
			},
			{
				name: 'HttpRequestNumber',
				value: '897',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:40.4955381+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f2f9a23f-efd1-4bc5-a316-bc603544c43a',
			},
			{
				name: 'HttpRequestNumber',
				value: '896',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:40.4490867+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '1894e914-d45b-436f-b125-56160011d80f',
			},
			{
				name: 'HttpRequestNumber',
				value: '895',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:35.4899092+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f092d759-7c3a-4e7e-a560-9ac7e6c77f7c',
			},
			{
				name: 'HttpRequestNumber',
				value: '894',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:35.4438695+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f02cb18d-8976-4f2c-a16a-a0c9006724a1',
			},
			{
				name: 'HttpRequestNumber',
				value: '893',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:30.4782757+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '743f4f0b-7e83-4eb8-bfdc-e3e97be0ecc6',
			},
			{
				name: 'HttpRequestNumber',
				value: '892',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:30.4324179+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '422d43b5-45a4-4b59-901b-399cb3fb448a',
			},
			{
				name: 'HttpRequestNumber',
				value: '891',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:25.4653028+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '7a3fe678-d6c9-475a-ab57-f7b4d06b8005',
			},
			{
				name: 'HttpRequestNumber',
				value: '890',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:25.4179895+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '956a5a79-b0dd-446b-b4d0-f9594d6f538d',
			},
			{
				name: 'HttpRequestNumber',
				value: '889',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:20.4554989+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '7cdab7b5-0d9b-4ae4-b023-83b5415e2909',
			},
			{
				name: 'HttpRequestNumber',
				value: '888',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:20.4074817+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'f9dbdf17-fc1a-45b4-bd88-030b2c26ec16',
			},
			{
				name: 'HttpRequestNumber',
				value: '887',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:15.4508724+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '9fef4e4a-e373-4a9a-b162-857af5c160f9',
			},
			{
				name: 'HttpRequestNumber',
				value: '886',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:15.4043773+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: '1b1c9faa-c0be-4722-b3f3-441e48d42c0a',
			},
			{
				name: 'HttpRequestNumber',
				value: '885',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:10.4501237+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"InternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'InternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: '1d9a0529-a75a-40cc-be5d-c871548ab233',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.EmptyRecycleBin (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000002e-0005-ef00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/EmptyRecycleBin',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '50',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'd8bedb8f-1151-4e2b-858f-286df530ffa2',
			},
			{
				name: 'HttpRequestNumber',
				value: '884',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
	{
		timestamp: '2023-02-14T11:58:10.4036882+00:00',
		level: 'Debug',
		messageTemplate: '{IndexName} searcher refreshed? {DidRefresh}',
		renderedMessage: '"ExternalIndex" searcher refreshed? False',
		properties: [
			{
				name: 'IndexName',
				value: 'ExternalIndex',
			},
			{
				name: 'DidRefresh',
				value: 'False',
			},
			{
				name: 'SourceContext',
				value: 'Examine.Lucene.Providers.LuceneIndex',
			},
			{
				name: 'ActionId',
				value: 'c4a4ecf3-34e7-4ff9-99ad-4aab0a23c3c2',
			},
			{
				name: 'ActionName',
				value: 'Umbraco.Cms.Web.BackOffice.Controllers.ContentController.DeleteById (Umbraco.Web.BackOffice)',
			},
			{
				name: 'RequestId',
				value: '4000005e-0004-fe00-b63f-84710c7967bb',
			},
			{
				name: 'RequestPath',
				value: '/umbraco/backoffice/umbracoapi/content/DeleteById',
			},
			{
				name: 'ProcessId',
				value: '17632',
			},
			{
				name: 'ProcessName',
				value: 'iisexpress',
			},
			{
				name: 'ThreadId',
				value: '57',
			},
			{
				name: 'ApplicationId',
				value: '0cf9334a65daa2e8a69943c6db2fb730bb14f2de',
			},
			{
				name: 'MachineName',
				value: 'DESKTOP-M35N63H',
			},
			{
				name: 'Log4NetLevel',
				value: 'DEBUG',
			},
			{
				name: 'HttpRequestId',
				value: 'cdfde20a-6e4e-4062-be6f-5daff839ed04',
			},
			{
				name: 'HttpRequestNumber',
				value: '883',
			},
			{
				name: 'HttpSessionId',
				value: '0',
			},
		],
		exception: null,
	},
];

const randomEnumValue = (enumeration: any): LogLevelModel => {
	const values = Object.keys(enumeration);
	const enumKey = values[Math.floor(Math.random() * values.length)];
	return enumeration[enumKey];
};

export const logs: LogMessageModel[] = allLogs.map((log) => {

	const randomLevel = randomEnumValue(LogLevelModel);

	return {
		...log,
		level: randomLevel,
		eventId: {
			TypeTag: null,
			Properties: [
				{
					Name: 'Id',
					Value: {
						Value: 17,
					},
				},
				{
					Name: 'Name',
					Value: {
						Value: 'ExceptionProcessingMessage',
					},
				},
			],
		},
		exception:
			randomLevel === LogLevelModel.ERROR
				? `System.InvalidOperationException: The identity did not contain requried claim name
		at Umbraco.Cloud.Identity.Cms.PrincipalExtensions.GetRequiredFirstValue(ClaimsIdentity identity, String claimType)
		at Umbraco.Cloud.Identity.Cms.ClaimsIdentityExtensions.ApplyNameClaim(ClaimsIdentity identity)
		at Umbraco.Cloud.Identity.Cms.ClaimsIdentityExtensions.ValidateAndTransformClaims(ClaimsIdentity identity, String currentPolicy, String[] userRoles, String passwordChangePolicy, String profilePolicy, String passwordResetPolicy)
		at Umbraco.Cloud.Identity.Cms.ClaimsIdentityExtensions.ValidateAndTransformClaims(ClaimsPrincipal principal, String currentPolicy, String[] userRoles, String passwordChangePolicy, String profilePolicy, String passwordResetPolicy)
		at Umbraco.Cloud.Identity.Cms.V10.OpenIdConnectEventHandler.OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
		at Microsoft.Identity.Web.MicrosoftIdentityWebAppAuthenticationBuilder.<>c__DisplayClass11_1.<<WebAppCallsWebApiImplementation>b__1>d.MoveNext()
	 --- End of stack trace from previous location ---
		at Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectHandler.RunAuthorizationCodeReceivedEventAsync(OpenIdConnectMessage authorizationResponse, ClaimsPrincipal user, AuthenticationProperties properties, JwtSecurityToken jwt)
		at Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectHandler.HandleRemoteAuthenticateAsync()`
				: undefined,
	};
});
