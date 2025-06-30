import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface ExampleCollectionItemModel {
	unique: string;
	entityType: string;
	name: string;
}

export interface ExampleCollectionFilterModel extends UmbCollectionFilterModel {}
