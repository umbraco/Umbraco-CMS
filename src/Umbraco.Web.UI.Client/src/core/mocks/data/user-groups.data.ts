import { UmbEntityData } from './entity.data';
import { PagedUserGroupPresentationModel, UserGroupPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// Temp mocked database
class UmbUserGroupsData extends UmbEntityData<UserGroupPresentationModel> {
	constructor(data: Array<UserGroupPresentationModel>) {
		super(data);
	}

	getAll(): PagedUserGroupPresentationModel {
		return {
			total: this.data.length,
			items: this.data,
		};
	}
}

export const data: Array<UserGroupPresentationModel> = [
	{
		$type: 'UserGroupPresentationModel',
		id: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
	},
];

export const umbUserGroupsData = new UmbUserGroupsData(data);
