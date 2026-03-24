import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface ExampleDynamicFacetCollectionFilterModel extends UmbCollectionFilterModel {}

export interface ExampleProductCollectionItemModel extends UmbCollectionItemModel {
	category: string;
	sizes: Array<string>;
	colors: Array<string>;
	price: number;
}
