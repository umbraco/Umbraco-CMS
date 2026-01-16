import type {
	UmbCollectionDataSource,
	UmbCollectionFilterModel,
	UmbCollectionItemModel,
} from '@umbraco-cms/backoffice/collection';

export type UmbLanguageCollectionDataSource = UmbCollectionDataSource<
	UmbPropertyEditorDataSourceCollectionItemModel,
	UmbPropertyEditorDataSourceCollectionFilterModel
>;

export interface UmbPropertyEditorDataSourceCollectionItemModel extends UmbCollectionItemModel {
	unique: string;
}

export interface UmbPropertyEditorDataSourceCollectionFilterModel extends UmbCollectionFilterModel {
	dataSourceTypes?: Array<string>;
}
