import type { UmbExtensionTypeEntityType } from '../entity.js';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbExtensionTypeCollectionFilterModel extends UmbCollectionFilterModel {}

export interface UmbExtensionTypeCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbExtensionTypeEntityType;
}
