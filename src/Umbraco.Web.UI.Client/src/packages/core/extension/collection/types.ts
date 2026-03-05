import type { UmbExtensionEntityType } from '../entity.js';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import type { UmbWithDescriptionModel } from '@umbraco-cms/backoffice/models';

export interface UmbExtensionCollectionFilterModel extends UmbCollectionFilterModel {
	/** @deprecated Use `extensionTypes` instead. Will be removed in v. 19 */
	type?: string;
	extensionTypes?: Array<string>;
}

export interface UmbExtensionCollectionItemModel extends UmbCollectionItemModel, UmbWithDescriptionModel {
	entityType: UmbExtensionEntityType;
	manifest: ManifestBase;
}
