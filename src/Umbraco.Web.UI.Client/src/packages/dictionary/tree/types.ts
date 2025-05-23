import type { UmbDictionaryEntityType, UmbDictionaryRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDictionaryTreeItemModel extends UmbTreeItemModel {
	entityType: UmbDictionaryEntityType;
}

export interface UmbDictionaryTreeRootModel extends UmbTreeRootModel {
	entityType: UmbDictionaryRootEntityType;
}
