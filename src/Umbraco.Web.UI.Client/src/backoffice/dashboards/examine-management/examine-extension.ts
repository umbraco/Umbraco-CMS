export interface SearcherModel {
	name: string;
	providerProperties: string[]; //TODO
}

export interface IndexModel {
	name: string;
	canRebuild: boolean;
	healthStatus: string;
	isHealthy: boolean;
	providerProperties: ProviderPropertiesModel;
}

export interface ProviderPropertiesModel {
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

export interface FieldViewModel {
	name: string;
	values: string[];
}

export interface SearchResultsModel {
	id: number;
	name: string;
	fields: FieldViewModel[];
	score: number;
}
