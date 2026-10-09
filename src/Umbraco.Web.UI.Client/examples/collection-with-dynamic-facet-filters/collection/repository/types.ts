import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export type ExampleDynamicFacetCollectionFilterModel = UmbCollectionFilterModel;

export interface ExampleProductCollectionItemModel extends UmbCollectionItemModel {
	name: string;
	category: string;
	sizes: Array<string>;
	colors: Array<string>;
	price: { amount: number; currency: string };
}
