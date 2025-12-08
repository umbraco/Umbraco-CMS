import type { UmbMemberTypeEntityType, UmbMemberTypeFolderEntityType, UmbMemberTypeRootEntityType } from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMemberTypeTreeItemModel extends UmbTreeItemModel {
	entityType: UmbMemberTypeEntityType | UmbMemberTypeFolderEntityType;
}

export interface UmbMemberTypeTreeRootModel extends UmbTreeRootModel {
	entityType: UmbMemberTypeRootEntityType;
}
