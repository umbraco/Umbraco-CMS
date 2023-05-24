import { UmbEntityData } from './entity.data.js';
import { PagedUserGroupResponseModel, UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';

// Temp mocked database
class UmbUserGroupsData extends UmbEntityData<UserGroupResponseModel> {
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
		$type: 'UserGroupResponseModel',
		id: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
	},
];

export const umbUserGroupsData = new UmbUserGroupsData(data);
