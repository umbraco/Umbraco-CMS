import type { UmbExtensionEntityType } from '../entity.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface UmbExtensionCollectionFilterModel extends UmbCollectionFilterModel {
	type?: string;
}

export interface UmbExtensionDetailModel extends ManifestBase {
	unique: string;
	entityType: UmbExtensionEntityType;
}
