import { UmbEntityData } from './entity.data.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_READ,
} from '@umbraco-cms/backoffice/document';
import {
	PagedUserGroupResponseModel,
	UserGroupItemResponseModel,
	UserGroupResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

const createUserGroupItem = (item: UserGroupResponseModel): UserGroupItemResponseModel => {
	return {
		name: item.name,
		id: item.id,
		icon: item.icon,
	};
};

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

export const data: Array<UserGroupResponseModel> = [
	{
		id: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: '9d24dc47-a4bf-427f-8a4a-b900f03b8a12',
		name: 'User Group 1',
		icon: 'umb:bell',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'f4626511-b0d7-4ab1-aebc-a87871a5dcfa',
		name: 'User Group 2',
		icon: 'umb:ball',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_READ],
	},
];

export const umbUserGroupData = new UmbUserGroupData(data);
