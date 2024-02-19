import type { UmbMemberTypeEntityType, UmbMemberTypeRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMemberTypeTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbMemberTypeEntityType;
}

export interface UmbMemberTypeTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbMemberTypeRootEntityType;
}
