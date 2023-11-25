import { UmbEntityData } from './entity.data.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_DELETE,
	UMB_USER_PERMISSION_DOCUMENT_READ,
} from '@umbraco-cms/backoffice/document';
import { UserGroupItemResponseModel, UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';

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
		icon: 'icon-medal',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'd813a006-385f-4350-a4fb-e0520776204b',
		name: 'Editors',
		icon: 'icon-tools',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'b735ea5c-a06a-4120-8ed3-e2f921ada42c',
		name: 'Sensitive data',
		icon: 'icon-lock',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'aecf2895-a017-4acf-9ece-f560939794f9',
		name: 'Translators',
		icon: 'icon-globe',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'e3d811dd-4dfc-496d-88e8-80734deb377f',
		name: 'Writers',
		icon: 'icon-edit',
		documentStartNodeId: 'all-property-editors-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b2',
		name: 'Something',
		icon: 'umb:medal',
		documentStartNodeId: 'simple-document-id',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: '9d24dc47-a4bf-427f-8a4a-b900f03b8a12',
		name: 'User Group 1',
		icon: 'icon-bell',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_DELETE],
	},
	{
		id: 'f4626511-b0d7-4ab1-aebc-a87871a5dcfa',
		name: 'User Group 2',
		icon: 'icon-ball',
		permissions: [UMB_USER_PERMISSION_DOCUMENT_READ],
	},
];

export const umbUserGroupData = new UmbUserGroupData(data);
