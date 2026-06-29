import type {
	IndexResponseModel,
	PagedIndexResponseModel,
	SearchResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { HealthStatusModel } from '@umbraco-cms/backoffice/external/backend-api';

export const Indexers: IndexResponseModel[] = [
	{
		name: 'ExternalIndex',
		canRebuild: true,
		healthStatus: { status: HealthStatusModel.HEALTHY },
		documentCount: 0,
		fieldCount: 0,
		searcherName: '',
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'StandardAnalyzer',
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /externalindex',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			EnableDefaultEventHandler: true,
			PublishedValuesOnly: true,
			SupportProtectedContent: false,
		},
	},
	{
		name: 'InternalIndex',
		canRebuild: true,
		healthStatus: { status: HealthStatusModel.HEALTHY },
		documentCount: 0,
		fieldCount: 0,
		searcherName: '',
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'CultureInvariantWhitespaceAnalyzer',
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /internalindex',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			EnableDefaultEventHandler: true,
			PublishedValuesOnly: false,
			SupportProtectedContent: true,
			IncludeFields: ['id', 'nodeName', 'updateDate', 'loginName', 'email', '__Key'],
		},
	},
	{
		name: 'MemberIndex',
		canRebuild: true,
		healthStatus: { status: HealthStatusModel.HEALTHY },
		fieldCount: 0,
		documentCount: 0,
		searcherName: '',
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'CultureInvariantWhitespaceAnalyzer',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			EnableDefaultEventHandler: true,
			IncludeFields: ['id', 'nodeName', 'updateDate', 'loginName', 'email', '__Key'],
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /membersindex',
			PublishedValuesOnly: false,
			SupportProtectedContent: false,
		},
	},
];

export const PagedIndexers: PagedIndexResponseModel = {
	items: Indexers,
	total: 0,
};

export const searchResultMockData: SearchResultResponseModel[] = [];
