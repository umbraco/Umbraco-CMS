import { UmbEntityData } from '../entity.data.js';
import { data } from './user-group.data.js';
import type { UserGroupItemResponseModel, UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';

const createUserGroupItem = (item: UserGroupResponseModel): UserGroupItemResponseModel => {
	return {
		name: item.name,
		id: item.id,
		icon: item.icon,
	};
};

class UmbUserGroupData extends UmbEntityData<UserGroupResponseModel> {
	constructor(data: Array<UserGroupResponseModel>) {
		super(data);
	}

	getItems(ids: Array<string>): Array<UserGroupItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createUserGroupItem(item));
	}

	/**
	 * Returns a list of permissions for the given user group ids
	 * @param {string[]} userGroupIds
	 * @return {*}  {string[]}
	 * @memberof UmbUserGroupData
	 */
	getPermissions(userGroupIds: string[]): string[] {
		const permissions = this.data
			.filter((userGroup) => userGroupIds.includes(userGroup.id || ''))
			.map((userGroup) => (userGroup.permissions?.length ? userGroup.permissions : []))
			.flat();

		// Remove duplicates
		return [...new Set(permissions)];
	}
}

export const umbUserGroupData = new UmbUserGroupData(data);
