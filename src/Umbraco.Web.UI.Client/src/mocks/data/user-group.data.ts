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
}

export const data: Array<UserGroupResponseModel> = [
	{
		id: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
		permissions: ['Umb.UserPermission.Document.Delete'],
	},
];

export const umbUserGroupData = new UmbUserGroupData(data);
