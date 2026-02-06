import type { UmbExtensionEntityType } from '../entity.js';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbExtensionCollectionFilterModel extends UmbCollectionFilterModel {
	type?: string;
}

export interface UmbExtensionCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbExtensionEntityType;
	manifest: ManifestBase;
}
