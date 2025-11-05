import type {
	IndexResponseModel,
	PagedIndexResponseModel,
	SearchResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { HealthStatusModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 *
 * @param indexName
 */
export function getIndexByName(indexName: string) {
	return Indexers.find((index) => {
		if (index.name) return index.name.toLocaleLowerCase() == indexName.toLocaleLowerCase();
		else return undefined;
	});
}

/**
 *
 */
export function getSearchResultsMockData(): SearchResultResponseModel[] {
	return searchResultMockData;
}

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

export const searchResultMockData: SearchResultResponseModel[] = [
	{
		id: '1',
		score: 1,
		fieldCount: 25,
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
		id: '2',
		score: 0.9,
		fieldCount: 25,
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
