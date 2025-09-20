import type { UmbCollectionDataSource, UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export type UmbLanguageCollectionDataSource = UmbCollectionDataSource<
	UmbPropertyEditorDataSourceCollectionItemModel,
	UmbPropertyEditorDataSourceCollectionFilterModel
>;

export interface UmbPropertyEditorDataSourceCollectionItemModel {
	entityType: string;
	unique: string;
	icon: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorDataSourceCollectionFilterModel extends UmbCollectionFilterModel {}
