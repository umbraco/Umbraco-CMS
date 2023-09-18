import { UmbEntityData } from './entity.data.js';
import { PagedUserGroupResponseModel, UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';

// Temp mocked database
class UmbUserGroupData extends UmbEntityData<UserGroupResponseModel> {
	constructor(data: Array<UserGroupResponseModel>) {
		super(data);
	}

	getAll(): PagedUserGroupResponseModel {
		return {
			total: this.data.length,
			items: this.data,
		};
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

export const data: Array<UserGroupResponseModel> = [
	{
		id: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
		permissions: ['Umb.UserPermission.Create', 'Umb.UserPermission.Delete'],
	},
	{
		id: '9d24dc47-a4bf-427f-8a4a-b900f03b8a12',
		name: 'User Group 1',
		icon: 'umb:star',
		permissions: ['Umb.UserPermission.Delete'],
	},
	{
		id: 'f4626511-b0d7-4ab1-aebc-a87871a5dcfa',
		name: 'User Group 2',
		icon: 'umb:star',
		permissions: ['Umb.UserPermission.Read'],
	},
];

export const umbUserGroupData = new UmbUserGroupData(data);
