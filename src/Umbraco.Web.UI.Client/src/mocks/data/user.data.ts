import { UmbEntityData } from './entity.data.js';
import { umbUserGroupData } from './user-group.data.js';
import { UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
import {
	PagedUserResponseModel,
	UserItemResponseModel,
	UserResponseModel,
	UserStateModel,
} from '@umbraco-cms/backoffice/backend-api';

const createUserItem = (item: UserResponseModel): UserItemResponseModel => {
	return {
		name: item.name,
		id: item.id,
	};
};

// Temp mocked database
class UmbUserData extends UmbEntityData<UserResponseModel> {
	constructor(data: UserResponseModel[]) {
		super(data);
	}

	getAll(): PagedUserResponseModel {
		return {
			total: this.data.length,
			items: this.data,
		};
	}

	getItems(ids: Array<string>): Array<UserItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createUserItem(item));
	}

	getCurrentUser(): UmbLoggedInUser {
		const firstUser = this.data[0];
		const permissions = firstUser.userGroupIds?.length ? umbUserGroupData.getPermissions(firstUser.userGroupIds) : [];

		return {
			id: firstUser.id,
			name: firstUser.name,
			email: firstUser.email,
			userName: firstUser.email,
			avatarUrls: [],
			hasAccessToAllLanguages: true,
			languageIsoCode: firstUser.languageIsoCode,
			languages: [],
			contentStartNodeIds: firstUser.contentStartNodeIds,
			mediaStartNodeIds: firstUser.mediaStartNodeIds,
			permissions,
		};
	}
}

export const data: Array<UserResponseModel & { type: string }> = [
	{
		id: 'bca6c733-a63d-4353-a271-9a8b6bcca8bd',
		type: 'user',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Umbraco User',
		email: 'noreply@umbraco.com',
		languageIsoCode: 'en-US',
		state: UserStateModel.ACTIVE,
		lastLoginDate: '9/10/2022',
		lastLockoutDate: '11/23/2021',
		lastPasswordChangeDate: '1/10/2022',
		updateDate: '2/10/2022',
		createDate: '3/13/2022',
		failedLoginAttempts: 946,
		userGroupIds: [
			'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
			'9d24dc47-a4bf-427f-8a4a-b900f03b8a12',
			'f4626511-b0d7-4ab1-aebc-a87871a5dcfa',
		],
	},
	{
		id: '82e11d3d-b91d-43c9-9071-34d28e62e81d',
		type: 'user',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Amelie Walker',
		email: 'awalker1@domain.com',
		languageIsoCode: 'Japanese',
		state: UserStateModel.INACTIVE,
		lastLoginDate: '4/12/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/1/2023',
		updateDate: '4/12/2023',
		createDate: '4/12/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: 'aa1d83a9-bc7f-47d2-b288-58d8a31f5017',
		type: 'user',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Oliver Kim',
		email: 'okim1@domain.com',
		languageIsoCode: 'Russian',
		state: UserStateModel.ACTIVE,
		lastLoginDate: '4/11/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/5/2023',
		updateDate: '4/11/2023',
		createDate: '4/11/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: 'ff2f4a50-d3d4-4bc4-869d-c7948c160e54',
		type: 'user',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Eliana Nieves',
		email: 'enieves1@domain.com',
		languageIsoCode: 'Spanish',
		state: UserStateModel.INVITED,
		lastLoginDate: '4/10/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/6/2023',
		updateDate: '4/10/2023',
		createDate: '4/10/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: 'c290c6d9-9f12-4838-8567-621b52a178de',
		type: 'user',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Jasmine Patel',
		email: 'jpatel1@domain.com',
		languageIsoCode: 'Hindi',
		state: UserStateModel.DISABLED,
		lastLoginDate: '4/9/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/7/2023',
		updateDate: '4/9/2023',
		createDate: '4/9/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
];

export const umbUsersData = new UmbUserData(data);
