import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbEntityData } from './entity.data.js';
import { umbUserGroupData } from './user-group.data.js';
import { UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
import {
	CreateUserRequestModel,
	CreateUserResponseModel,
	InviteUserRequestModel,
	UpdateUserGroupsOnUserRequestModel,
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

	/**
	 * Create user
	 * @param {CreateUserRequestModel} data
	 * @memberof UmbUserData
	 */
	createUser = (data: CreateUserRequestModel): CreateUserResponseModel => {
		const userId = UmbId.new();
		const initialPassword = 'mocked-initial-password';

		const user: UserResponseModel = {
			id: userId,
			languageIsoCode: null,
			contentStartNodeIds: [],
			mediaStartNodeIds: [],
			avatarUrls: [],
			state: UserStateModel.INACTIVE,
			failedLoginAttempts: 0,
			createDate: new Date().toUTCString(),
			updateDate: new Date().toUTCString(),
			lastLoginDate: null,
			lastLockoutDate: null,
			lastPasswordChangeDate: null,
			...data,
		};

		this.insert(user);

		return { userId, initialPassword };
	};

	/**
	 * Get user items
	 * @param {Array<string>} ids
	 * @return {*}  {Array<UserItemResponseModel>}
	 * @memberof UmbUserData
	 */
	getItems(ids: Array<string>): Array<UserItemResponseModel> {
		const items = this.data.filter((item) => ids.includes(item.id ?? ''));
		return items.map((item) => createUserItem(item));
	}

	/**
	 * Set user groups
	 * @param {UpdateUserGroupsOnUserRequestModel} data
	 * @memberof UmbUserData
	 */
	setUserGroups(data: UpdateUserGroupsOnUserRequestModel): void {
		const users = this.data.filter((user) => data.userIds?.includes(user.id ?? ''));
		users.forEach((user) => {
			user.userGroupIds = data.userGroupIds;
		});
	}

	/**
	 * Get current user
	 * @return {*}  {UmbLoggedInUser}
	 * @memberof UmbUserData
	 */
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

	/**
	 * Disable users
	 * @param {Array<string>} ids
	 * @memberof UmbUserData
	 */
	disable(ids: Array<string>): void {
		const users = this.data.filter((user) => ids.includes(user.id ?? ''));
		users.forEach((user) => {
			user.state = UserStateModel.DISABLED;
		});
	}

	/**
	 * Enable users
	 * @param {Array<string>} ids
	 * @memberof UmbUserData
	 */
	enable(ids: Array<string>): void {
		const users = this.data.filter((user) => ids.includes(user.id ?? ''));
		users.forEach((user) => {
			user.state = UserStateModel.ACTIVE;
		});
	}

	/**
	 * Unlock users
	 * @param {Array<string>} ids
	 * @memberof UmbUserData
	 */
	unlock(ids: Array<string>): void {
		const users = this.data.filter((user) => ids.includes(user.id ?? ''));
		users.forEach((user) => {
			user.failedLoginAttempts = 0;
			user.state = UserStateModel.ACTIVE;
		});
	}

	invite(data: InviteUserRequestModel): void {
		const invitedUser = {
			status: UserStateModel.INVITED,
			...data,
		};

		this.createUser(invitedUser);
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
		contentStartNodeIds: ['simple-document-id'],
		mediaStartNodeIds: ['f2f81a40-c989-4b6b-84e2-057cecd3adc1'],
		name: 'Amelie Walker',
		email: 'awalker1@domain.com',
		languageIsoCode: 'Japanese',
		state: UserStateModel.INACTIVE,
		lastLoginDate: '2023-10-12T18:30:32.879Z',
		lastLockoutDate: null,
		lastPasswordChangeDate: '2023-10-12T18:30:32.879Z',
		updateDate: '2023-10-12T18:30:32.879Z',
		createDate: '2023-10-12T18:30:32.879Z',
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
		lastLoginDate: '2023-10-12T18:30:32.879Z',
		lastLockoutDate: null,
		lastPasswordChangeDate: '2023-10-12T18:30:32.879Z',
		updateDate: '2023-10-12T18:30:32.879Z',
		createDate: '2023-10-12T18:30:32.879Z',
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
		lastLoginDate: '2023-10-12T18:30:32.879Z',
		lastLockoutDate: null,
		lastPasswordChangeDate: null,
		updateDate: '2023-10-12T18:30:32.879Z',
		createDate: '2023-10-12T18:30:32.879Z',
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
		state: UserStateModel.LOCKED_OUT,
		lastLoginDate: '2023-10-12T18:30:32.879Z',
		lastLockoutDate: '2023-10-12T18:30:32.879Z',
		lastPasswordChangeDate: null,
		updateDate: '2023-10-12T18:30:32.879Z',
		createDate: '2023-10-12T18:30:32.879Z',
		failedLoginAttempts: 25,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
];

export const umbUsersData = new UmbUserData(data);
