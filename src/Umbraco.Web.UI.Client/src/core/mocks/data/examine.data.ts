import {
	IndexModel,
	SearchResultsModel,
	SearcherModel,
} from 'src/backoffice/dashboards/examine-management/examine-extension';

export function getIndexByName(indexName: string) {
	return Indexers.find((index) => {
		return index.name.toLocaleLowerCase() == indexName.toLocaleLowerCase();
	});
}

export function getIndexers(): IndexModel[] {
	return Indexers;
}

export function getSearchResultsMockData(): SearchResultsModel[] {
	return searchResultMockData;
}

const Indexers: IndexModel[] = [
	{
		name: 'ExternalIndex',
		canRebuild: true,
		healthStatus: 'Healthy',
		isHealthy: true,
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'StandardAnalyzer',
			DocumentCount: 0,
			FieldCount: 0,
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
		healthStatus: 'Healthy',
		isHealthy: true,
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'CultureInvariantWhitespaceAnalyzer',
			DocumentCount: 0,
			FieldCount: 0,
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
		healthStatus: 'Healthy',
		isHealthy: true,
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'CultureInvariantWhitespaceAnalyzer',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			DocumentCount: 0,
			EnableDefaultEventHandler: true,
			FieldCount: 0,
			IncludeFields: ['id', 'nodeName', 'updateDate', 'loginName', 'email', '__Key'],
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /membersindex',
			PublishedValuesOnly: false,
			SupportProtectedContent: false,
		},
	},
];

export const searchResultMockData: SearchResultsModel[] = [
	{
		id: 1,
		name: 'Home',
		score: 1,
		fields: [
			{ name: '__Icon', values: ['icon-document'] },
			{ name: '__IndexType', values: ['content'] },
			{ name: '__Key', values: ['903wyrqwjf-33wrefef-wefwef3-erw'] },
			{ name: '__NodeId', values: ['1059'] },
			{ name: '__NodeTypeAlias', values: ['Home'] },
			{ name: '__Path', values: ['-1.345'] },
			{ name: '__Published', values: ['y'] },
			{ name: '__VariesByCulture', values: ['n'] },
			{ name: 'createDate', values: ['30752539'] },
			{ name: 'creatorId', values: ['-1'] },
			{ name: 'creatorName', values: ['Lone'] },
			{ name: 'icon', values: ['icon-document'] },
			{ name: 'id', values: ['1059'] },
			{ name: 'image', values: ['34343-3wdsw-sd35-3s', 'afewr-q5-23rd-3red'] },
			{ name: 'level', values: ['1'] },
			{ name: 'nodeName', values: ['Just a picture'] },
			{ name: 'nodeType', values: ['1056'] },
			{ name: 'parentID', values: ['-1'] },
			{ name: 'path', values: ['-3.325'] },
			{ name: 'sortOrder', values: ['1'] },
			{ name: 'templateID', values: ['1055'] },
			{ name: 'updateDate', values: ['9573024532945'] },
			{ name: 'urlName', values: ['just-a-picture'] },
			{ name: 'writerID', values: ['-1'] },
			{ name: 'writerName', values: ['Lone'] },
		],
	},
	{
		id: 2,
		name: 'Dojo',
		score: 0.9,
		fields: [
			{ name: '__Icon', values: ['icon-document'] },
			{ name: '__IndexType', values: ['content'] },
			{ name: '__Key', values: ['903wyrqwjf-33wrefef-wefwef3-erw'] },
			{ name: '__NodeId', values: ['1059'] },
			{ name: '__NodeTypeAlias', values: ['Home'] },
			{ name: '__Path', values: ['-1.345'] },
			{ name: '__Published', values: ['y'] },
			{ name: '__VariesByCulture', values: ['n'] },
			{ name: 'createDate', values: ['30752539'] },
			{ name: 'creatorId', values: ['-1'] },
			{ name: 'creatorName', values: ['Lone'] },
			{ name: 'icon', values: ['icon-document'] },
			{ name: 'id', values: ['1059'] },
			{ name: 'image', values: ['34343-3wdsw-sd35-3s', 'afewr-q5-23rd-3red'] },
			{ name: 'level', values: ['1'] },
			{ name: 'nodeName', values: ['Just a picture'] },
			{ name: 'nodeType', values: ['1056'] },
			{ name: 'parentID', values: ['-1'] },
			{ name: 'path', values: ['-3.325'] },
			{ name: 'sortOrder', values: ['1'] },
			{ name: 'templateID', values: ['1055'] },
			{ name: 'updateDate', values: ['9573024532945'] },
			{ name: 'urlName', values: ['just-a-picture'] },
			{ name: 'writerID', values: ['-1'] },
			{ name: 'writerName', values: ['Lone'] },
		],
	},
];
