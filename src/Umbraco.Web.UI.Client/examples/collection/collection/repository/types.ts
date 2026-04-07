import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbWithDescriptionModel } from '@umbraco-cms/backoffice/models';

export interface ExampleCollectionItemModel extends UmbCollectionItemModel, UmbWithDescriptionModel {
	name: string;
	icon: string;
}

export interface ExampleCollectionFilterModel extends UmbCollectionFilterModel {}
