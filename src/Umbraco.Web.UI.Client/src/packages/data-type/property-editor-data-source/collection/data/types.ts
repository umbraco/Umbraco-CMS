import type {
	UmbCollectionDataSource,
	UmbCollectionFilterModel,
	UmbCollectionItemModel,
} from '@umbraco-cms/backoffice/collection';

export type UmbLanguageCollectionDataSource = UmbCollectionDataSource<
	UmbPropertyEditorDataSourceCollectionItemModel,
	UmbPropertyEditorDataSourceCollectionFilterModel
>;

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourceCollectionItemModel extends UmbCollectionItemModel {
	unique: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourceCollectionFilterModel extends UmbCollectionFilterModel {
	dataSourceTypes?: Array<string>;
}
