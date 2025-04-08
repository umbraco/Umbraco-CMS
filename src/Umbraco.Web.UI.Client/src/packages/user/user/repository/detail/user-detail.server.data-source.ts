import type { UmbUserDetailModel, UmbUserStartNodesModel } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import { UmbUserKind } from '../../utils/user-kind.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateUserRequestModel,
	UpdateUserRequestModel,
	UserKindModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @class UmbUserServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserServerDataSource implements UmbDetailDataSource<UmbUserDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new User scaffold
	 * @param {(string | null)} parentUnique
	 * @returns { CreateUserRequestModel }
	 * @memberof UmbUserServerDataSource
	 */
	async createScaffold() {
		const data: UmbUserDetailModel = {
			avatarUrls: [],
			createDate: null,
			documentStartNodeUniques: [],
			email: '',
			entityType: UMB_USER_ENTITY_TYPE,
			failedLoginAttempts: 0,
			hasDocumentRootAccess: false,
			hasMediaRootAccess: false,
			isAdmin: false,
			kind: UmbUserKind.DEFAULT,
			languageIsoCode: '',
			lastLockoutDate: null,
			lastLoginDate: null,
			lastPasswordChangeDate: null,
			mediaStartNodeUniques: [],
			name: '',
			state: null,
			unique: UmbId.new(),
			updateDate: null,
			userGroupUniques: [],
			userName: '',
		};

		return { data };
	}

	/**
	 * Fetches a User with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbUserServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, UserService.getUserById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const user: UmbUserDetailModel = {
			avatarUrls: data.avatarUrls,
			createDate: data.createDate,
			hasDocumentRootAccess: data.hasDocumentRootAccess,
			documentStartNodeUniques: data.documentStartNodeIds.map((node) => {
				return {
					unique: node.id,
				};
			}),
			email: data.email,
			entityType: UMB_USER_ENTITY_TYPE,
			failedLoginAttempts: data.failedLoginAttempts,
			isAdmin: data.isAdmin,
			kind: data.kind,
			languageIsoCode: data.languageIsoCode || null,
			lastLockoutDate: data.lastLockoutDate || null,
			lastLoginDate: data.lastLoginDate || null,
			lastPasswordChangeDate: data.lastPasswordChangeDate || null,
			hasMediaRootAccess: data.hasMediaRootAccess,
			mediaStartNodeUniques: data.mediaStartNodeIds.map((node) => {
				return {
					unique: node.id,
				};
			}),
			name: data.name,
			state: data.state,
			unique: data.id,
			updateDate: data.updateDate,
			userGroupUniques: data.userGroupIds.map((reference) => {
				return {
					unique: reference.id,
				};
			}),
			userName: data.userName,
		};

		return { data: user };
	}

	/**
	 * Inserts a new User on the server
	 * @param {UmbUserDetailModel} model
	 * @returns {*}
	 * @memberof UmbUserServerDataSource
	 */
	async create(model: UmbUserDetailModel) {
		if (!model) throw new Error('User is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateUserRequestModel = {
			email: model.email,
			name: model.name,
			userGroupIds: model.userGroupUniques.map((reference) => {
				return {
					id: reference.unique,
				};
			}),
			userName: model.userName,
			kind: model.kind as UserKindModel,
		};

		const { data, error } = await tryExecute(
			this.#host,
			UserService.postUser({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a User on the server
	 * @param {UmbUserDetailModel} User
	 * @param model
	 * @returns {*}
	 * @memberof UmbUserServerDataSource
	 */
	async update(model: UmbUserDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateUserRequestModel = {
			documentStartNodeIds: model.documentStartNodeUniques.map((node) => {
				return {
					id: node.unique,
				};
			}),
			email: model.email,
			hasDocumentRootAccess: model.hasDocumentRootAccess,
			hasMediaRootAccess: model.hasMediaRootAccess,
			languageIsoCode: model.languageIsoCode || '',
			mediaStartNodeIds: model.mediaStartNodeUniques.map((node) => {
				return {
					id: node.unique,
				};
			}),
			name: model.name,
			userGroupIds: model.userGroupUniques.map((reference) => {
				return {
					id: reference.unique,
				};
			}),
			userName: model.userName,
		};

		const { error } = await tryExecute(
			this.#host,
			UserService.putUserById({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a User on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbUserServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			UserService.deleteUserById({
				id: unique,
			}),
		);
	}

	/**
	 * Calculates the start nodes for the User
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbUserServerDataSource
	 */
	async calculateStartNodes(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			UserService.getUserByIdCalculateStartNodes({
				id: unique,
			}),
		);

		if (data) {
			const calculatedStartNodes: UmbUserStartNodesModel = {
				hasDocumentRootAccess: data.hasDocumentRootAccess,
				documentStartNodeUniques: data.documentStartNodeIds.map((node) => {
					return {
						unique: node.id,
					};
				}),
				hasMediaRootAccess: data.hasMediaRootAccess,
				mediaStartNodeUniques: data.mediaStartNodeIds.map((node) => {
					return {
						unique: node.id,
					};
				}),
			};

			return { data: calculatedStartNodes };
		}

		return { error };
	}
}
