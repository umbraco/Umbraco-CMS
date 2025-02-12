import { umbUserGroupMockDb } from '../user-group/user-group.db.js';
import { stringFilter, queryFilter, objectArrayFilter } from '../utils.js';
import { UmbEntityMockDbBase } from '../utils/entity/entity-base.js';
import { UmbMockEntityItemManager } from '../utils/entity/entity-item.manager.js';
import { UmbMockEntityDetailManager } from '../utils/entity/entity-detail.manager.js';
import type { UmbMockUserModel } from './user.data.js';
import { data, mfaLoginProviders } from './user.data.js';
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

interface UserFilterOptions {
	skip: number;
	take: number;
	orderBy: string;
	orderDirection: string;
	userGroupIds: Array<{ id: string }>;
	userStates: Array<string>;
	filter: string;
}

const userGroupFilter = (filterOptions: UserFilterOptions, item: UmbMockUserModel) =>
	objectArrayFilter(filterOptions.userGroupIds, item.userGroupIds, 'id');
const userStateFilter = (filterOptions: UserFilterOptions, item: UmbMockUserModel) =>
	stringFilter(filterOptions.userStates, item.state);
const userQueryFilter = (filterOptions: UserFilterOptions, item: UmbMockUserModel) =>
	queryFilter(filterOptions.filter, item.name);

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
		const users = this.data.filter((user) => data.userIds?.map((reference) => reference.id).includes(user.id));

		users.forEach((user) => {
			user.userGroupIds = data.userGroupIds;
		});
	}

	/**
	 * Get current user
	 * @returns {*}  {UmbCurrentUser}
	 * @memberof UmbUserData
	 */
	getCurrentUser(): CurrentUserResponseModel {
		const firstUser = this.data[0];
		const permissions = firstUser.userGroupIds?.length ? umbUserGroupMockDb.getPermissions(firstUser.userGroupIds) : [];
		const fallbackPermissions = firstUser.userGroupIds?.length
			? umbUserGroupMockDb.getFallbackPermissions(firstUser.userGroupIds)
			: [];
		const allowedSections = firstUser.userGroupIds?.length
			? umbUserGroupMockDb.getAllowedSections(firstUser.userGroupIds)
			: [];

		return {
			id: firstUser.id,
			name: firstUser.name,
			email: firstUser.email,
			userName: firstUser.email,
			hasAccessToSensitiveData: true,
			avatarUrls: [],
			hasAccessToAllLanguages: true,
			languageIsoCode: firstUser.languageIsoCode || null,
			languages: [],
			documentStartNodeIds: firstUser.documentStartNodeIds,
			mediaStartNodeIds: firstUser.mediaStartNodeIds,
			hasDocumentRootAccess: firstUser.hasDocumentRootAccess,
			hasMediaRootAccess: firstUser.hasMediaRootAccess,
			fallbackPermissions,
			permissions,
			allowedSections,
			isAdmin: firstUser.isAdmin,
			userGroupIds: firstUser.userGroupIds,
		};
	}

	getMfaLoginProviders() {
		return mfaLoginProviders;
	}

	enableMfaProvider(providerName: string) {
		const provider = mfaLoginProviders.find((x) => x.providerName === providerName);
		if (provider) {
			provider.isEnabledOnUser = true;
			return true;
		}

		return false;
	}

	disableMfaProvider(providerName: string) {
		const provider = mfaLoginProviders.find((x) => x.providerName === providerName);
		if (provider) {
			provider.isEnabledOnUser = false;
			return true;
		}

		return false;
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

	filter(options: UserFilterOptions): PagedUserResponseModel {
		const allItems = this.getAll();

		const filterOptions: UserFilterOptions = {
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
		avatarUrls: item.avatarUrls,
		id: item.id,
		kind: item.kind,
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
		hasDocumentRootAccess: false,
		hasMediaRootAccess: false,
		avatarUrls: [],
		state: UserStateModel.INACTIVE,
		failedLoginAttempts: 0,
		createDate: new Date().toUTCString(),
		updateDate: new Date().toUTCString(),
		lastLoginDate: null,
		lastLockoutDate: null,
		lastPasswordChangeDate: null,
		isAdmin: item.userGroupIds.map((reference) => reference.id).includes(umbUserGroupMockDb.getAll()[0].id),
		kind: item.kind,
	};
};

const detailResponseMapper = (item: UmbMockUserModel): UserResponseModel => {
	return {
		avatarUrls: item.avatarUrls,
		createDate: item.createDate,
		documentStartNodeIds: item.documentStartNodeIds,
		email: item.email,
		failedLoginAttempts: item.failedLoginAttempts,
		hasDocumentRootAccess: item.hasDocumentRootAccess,
		hasMediaRootAccess: item.hasMediaRootAccess,
		id: item.id,
		isAdmin: item.isAdmin,
		kind: item.kind,
		languageIsoCode: item.languageIsoCode,
		lastLockoutDate: item.lastLockoutDate,
		lastLoginDate: item.lastLoginDate,
		lastPasswordChangeDate: item.lastPasswordChangeDate,
		mediaStartNodeIds: item.mediaStartNodeIds,
		name: item.name,
		state: item.state,
		updateDate: item.updateDate,
		userGroupIds: item.userGroupIds,
		userName: item.userName,
	};
};

export const umbUserMockDb = new UmbUserMockDB(data);
