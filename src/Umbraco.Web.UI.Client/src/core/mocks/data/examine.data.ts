export function getIndexByName(indexName: string) {
	return Indexers.find((index) => {
		return index.name.toLocaleLowerCase() == indexName.toLocaleLowerCase();
	});
}

export function getIndexers() {
	return Indexers;
}

export function searchResFromIndex() {
	return ResultsFromIndex;
}

export interface Searcher {
	name: string;
	providerProperties: string[];
}

export interface Indexer {
	name: string;
	canRebuild: boolean;
	healthStatus: Health;
	isHealthy: boolean;
	providerProperties: ProviderProperties;
}

export interface IndexDisplay {
	name: string;
	healthStatus: Health;
}

export interface ProviderProperties {
	CommitCount: number;
	DefaultAnalyzer: string;
	DocumentCount: number;
	FieldCount: number;
	LuceneDirectory: string;
	LuceneIndexFolder: string;
	DirectoryFactory: string;
	EnableDefaultEventHandler: boolean;
	PublishedValuesOnly: boolean;
	SupportProtectedContent: boolean;
	IncludeFields?: string[];
}

export interface SearchResult {
	id: number;
	name: string;
	fields: any;
	score: number;
}

export default interface DocumentImageFieldKeys {
	key: string;
	mediaKey: string;
}

type Health = 'Healthy' | 'Unhealthy';

const Indexers: Indexer[] = [
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

const ResultsFromIndex: SearchResult[] = [
	{
		id: 1,
		name: 'Home',
		fields: {
			__Icon: 'icon-document',
			__IndexType: 'content',
			__Key: '903wyrqwjf-33wrefef-wefwef3-erw',
			__NodeId: 1059,
			__NodeTypeAlias: 'Home',
			__Path: -1.345,
			__Published: 'y',
			__VariesByCulture: 'n',
			createDate: 30752539,
			creatorId: -1,
			creatorName: 'Lone',
			icon: 'icon-document',
			id: 1059,
			image: [{ key: '34343-3wdsw-sd35-3s', mediaKey: 'afewr-q5-23rd-3red' }],
			level: 1,
			nodeName: 'Just a picture',
			nodeType: 1056,
			parentID: -1,
			path: -3.325,
			sortOrder: 1,
			templateID: 1055,
			updateDate: 9573024532945,
			urlName: 'just-a-picture',
			writerID: -1,
			writerName: 'Lone',
		},
		score: 1,
	},
	{
		id: 2,
		name: 'NotHome',
		score: 0.1,
		fields: {
			__Icon: 'icon-document',
			__IndexType: 'content',
			__Key: '903wyrqwjf-33wrefef-wefwef3-erw',
			__NodeId: 1059,
			__NodeTypeAlias: 'Home',
			__Path: -1.345,
			__Published: 'y',
			__VariesByCulture: 'n',
			createDate: 30752539,
			creatorId: -1,
			creatorName: 'Lone',
			icon: 'icon-document',
			id: 1059,
			image: [{ key: '34343-3wdsw-sd35-3s', mediaKey: 'afewr-q5-23rd-3red' }],
			level: 1,
			nodeName: 'Just a picture',
			nodeType: 1056,
			parentID: -1,
			path: -3.325,
			sortOrder: 1,
			templateID: 1055,
			updateDate: 9573024532945,
			urlName: 'just-a-picture',
			writerID: -1,
			writerName: 'Lone',
		},
	},
];
