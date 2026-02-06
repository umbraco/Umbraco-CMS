import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

export interface UmbExtensionCollectionItemModel extends UmbCollectionItemModel {
	unique: string;
	name: string;
	type: string;
}

export interface UmbExtensionCollectionFilterModel extends UmbCollectionFilterModel {
	extensionTypes?: Array<string>;
}
