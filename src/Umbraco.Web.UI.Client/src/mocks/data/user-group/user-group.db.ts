import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import type { UmbMockUserGroupModel } from './user-group.data.js';
import { data } from './user-group.data.js';
import type {
	CreateUserGroupRequestModel,
	DocumentPermissionPresentationModel,
	UnknownTypePermissionPresentationModel,
	UserGroupItemResponseModel,
	UserGroupResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

export class UmbUserGroupMockDB extends UmbEntityMockDbBase<UmbMockUserGroupModel> {
	item = new UmbMockEntityItemManager<UmbMockUserGroupModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockUserGroupModel>(this, createMockMapper, detailResponseMapper);

	constructor(data: Array<UmbMockUserGroupModel>) {
		super(data);
	}

	/**
	 * Returns a list of permissions for the given user group ids
	 * @param {string[]} userGroupIds
	 * @return {*}  {string[]}
	 * @memberof UmbUserGroupData
	 */
	getPermissions(
		userGroupIds: string[],
	): Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel> {
		const permissions = this.data
			.filter((userGroup) => userGroupIds.includes(userGroup.id))
			.map((userGroup) => (userGroup.permissions?.length ? userGroup.permissions : []))
			.flat();

		// Remove duplicates
		const uniqueArray = Array.from(new Set(permissions.map((e) => JSON.stringify(e)))).map((e) => JSON.parse(e));
		return uniqueArray;
	}
}

const itemMapper = (item: UmbMockUserGroupModel): UserGroupItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
		icon: item.icon,
	};
};

const createMockMapper = (item: CreateUserGroupRequestModel): UmbMockUserGroupModel => {
	return {
		documentRootAccess: item.documentRootAccess,
		documentStartNode: item.documentStartNode,
		hasAccessToAllLanguages: item.hasAccessToAllLanguages,
		icon: item.icon,
		id: UmbId.new(),
		isSystemGroup: false,
		languages: item.languages,
		mediaRootAccess: item.mediaRootAccess,
		mediaStartNode: item.mediaStartNode,
		name: item.name,
		fallbackPermissions: item.fallbackPermissions,
		permissions: item.permissions,
		sections: item.sections,
	};
};

const detailResponseMapper = (item: UmbMockUserGroupModel): UserGroupResponseModel => {
	return {
		documentRootAccess: item.documentRootAccess,
		documentStartNode: item.documentStartNode,
		hasAccessToAllLanguages: item.hasAccessToAllLanguages,
		icon: item.icon,
		id: item.id,
		isSystemGroup: item.isSystemGroup,
		languages: item.languages,
		mediaRootAccess: item.mediaRootAccess,
		mediaStartNode: item.mediaStartNode,
		name: item.name,
		fallbackPermissions: item.fallbackPermissions,
		permissions: item.permissions,
		sections: item.sections,
	};
};

export const umbUserGroupMockDb = new UmbUserGroupMockDB(data);
