import type { UmbDictionaryEntityType, UmbDictionaryRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDictionaryTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDictionaryEntityType;
}

export interface UmbDictionaryTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDictionaryRootEntityType;
}
