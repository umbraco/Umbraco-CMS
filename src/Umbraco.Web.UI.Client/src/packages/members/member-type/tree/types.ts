import type { UmbMemberTypeEntityType, UmbMemberTypeRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMemberTypeTreeItemModel extends UmbTreeItemModel {
	entityType: UmbMemberTypeEntityType;
}

export interface UmbMemberTypeTreeRootModel extends UmbTreeRootModel {
	entityType: UmbMemberTypeRootEntityType;
}
