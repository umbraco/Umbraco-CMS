import { UmbEntityData } from '../entity.data.js';
import { umbUserGroupMockDb } from '../user-group/user-group.db.js';
import { arrayFilter, stringFilter, queryFilter } from '../utils.js';
import { data } from './user.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbCurrentUser } from '@umbraco-cms/backoffice/current-user';
import type {
	CreateUserRequestModel,
	CreateUserResponseModel,
	InviteUserRequestModel,
	PagedUserResponseModel,
	UpdateUserGroupsOnUserRequestModel,
	UserItemResponseModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

const createUserItem = (item: UserResponseModel): UserItemResponseModel => {
	return {
		name: item.name,
		id: item.id,
	};
};

const userGroupFilter = (filterOptions: any, item: UserResponseModel) =>
	arrayFilter(filterOptions.userGroupIds, item.userGroupIds);
const userStateFilter = (filterOptions: any, item: UserResponseModel) =>
	stringFilter(filterOptions.userStates, item.state);
const userQueryFilter = (filterOptions: any, item: UserResponseModel) => queryFilter(filterOptions.filter, item.name);

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
	getCurrentUser(): UmbCurrentUser {
		const firstUser = this.data[0];
		const permissions = firstUser.userGroupIds?.length ? umbUserGroupMockDb.getPermissions(firstUser.userGroupIds) : [];

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

	/**
	 * Invites a user
	 * @param {InviteUserRequestModel} data
	 * @memberof UmbUserData
	 */
	invite(data: InviteUserRequestModel) {
		const invitedUser = {
			...data,
			state: UserStateModel.INVITED,
		};

		const response = this.createUser(invitedUser);

		return { userId: response.userId };
	}

	filter(options: any): PagedUserResponseModel {
		const allItems = this.getAll();

		const filterOptions = {
			skip: options.skip || 0,
			take: options.take || 25,
			orderBy: options.orderBy || 'name',
			orderDirection: options.orderDirection || 'asc',
			userGroupIds: options.userGroupIds,
			userStates: options.userStates,
			filter: options.filter,
		};

		const filteredItems = allItems.filter(
			(item) =>
				userGroupFilter(filterOptions, item) &&
				userStateFilter(filterOptions, item) &&
				userQueryFilter(filterOptions, item),
		);
		const totalItems = filteredItems.length;

		const paginatedItems = filteredItems.slice(filterOptions.skip, filterOptions.skip + filterOptions.take);

		return { total: totalItems, items: paginatedItems };
	}
}

export const umbUsersData = new UmbUserData(data);
