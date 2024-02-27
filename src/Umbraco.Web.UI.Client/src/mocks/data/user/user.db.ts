import { umbUserGroupMockDb } from '../user-group/user-group.db.js';
import { arrayFilter, stringFilter, queryFilter } from '../utils.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockUserModel } from './user.data.js';
import { data } from './user.data.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	CreateUserRequestModel,
	CurrentUserResponseModel,
	InviteUserRequestModel,
	PagedUserResponseModel,
	UpdateUserGroupsOnUserRequestModel,
	UserItemResponseModel,
	UserResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

const userGroupFilter = (filterOptions: any, item: UmbMockUserModel) =>
	arrayFilter(filterOptions.userGroupIds, item.userGroupIds);
const userStateFilter = (filterOptions: any, item: UmbMockUserModel) =>
	stringFilter(filterOptions.userStates, item.state);
const userQueryFilter = (filterOptions: any, item: UmbMockUserModel) => queryFilter(filterOptions.filter, item.name);

// Temp mocked database
class UmbUserMockDB extends UmbEntityMockDbBase<UmbMockUserModel> {
	item = new UmbMockEntityItemManager<UmbMockUserModel>(this, itemMapper);
	detail = new UmbMockEntityDetailManager<UmbMockUserModel>(this, createMockMapper, detailResponseMapper);

	constructor(data: UmbMockUserModel[]) {
		super(data);
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
	 * @return {*}  {UmbCurrentUser}
	 * @memberof UmbUserData
	 */
	getCurrentUser(): CurrentUserResponseModel {
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
			documentStartNodeIds: firstUser.documentStartNodeIds,
			mediaStartNodeIds: firstUser.mediaStartNodeIds,
			fallbackPermissions: [],
			permissions,
			allowedSections: [],
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
		const users = this.data.filter((user) => ids.includes(user.id));
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
		const users = this.data.filter((user) => ids.includes(user.id));
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

		const newUserId = this.detail.create(invitedUser);

		return { userId: newUserId };
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

const itemMapper = (item: UmbMockUserModel): UserItemResponseModel => {
	return {
		id: item.id,
		name: item.name,
	};
};

const createMockMapper = (item: CreateUserRequestModel): UmbMockUserModel => {
	return {
		email: item.email,
		userName: item.userName,
		name: item.name,
		userGroupIds: item.userGroupIds,
		id: UmbId.new(),
		languageIsoCode: null,
		documentStartNodeIds: [],
		mediaStartNodeIds: [],
		avatarUrls: [],
		state: UserStateModel.INACTIVE,
		failedLoginAttempts: 0,
		createDate: new Date().toUTCString(),
		updateDate: new Date().toUTCString(),
		lastLoginDate: null,
		lastLockoutDate: null,
		lastPasswordChangeDate: null,
	};
};

const detailResponseMapper = (item: UmbMockUserModel): UserResponseModel => {
	return {
		email: item.email,
		userName: item.userName,
		name: item.name,
		userGroupIds: item.userGroupIds,
		id: item.id,
		languageIsoCode: item.languageIsoCode,
		documentStartNodeIds: item.documentStartNodeIds,
		mediaStartNodeIds: item.mediaStartNodeIds,
		avatarUrls: item.avatarUrls,
		state: item.state,
		failedLoginAttempts: item.failedLoginAttempts,
		createDate: item.createDate,
		updateDate: item.updateDate,
		lastLoginDate: item.lastLoginDate,
		lastLockoutDate: item.lastLockoutDate,
		lastPasswordChangeDate: item.lastPasswordChangeDate,
	};
};

export const umbUserMockDb = new UmbUserMockDB(data);
