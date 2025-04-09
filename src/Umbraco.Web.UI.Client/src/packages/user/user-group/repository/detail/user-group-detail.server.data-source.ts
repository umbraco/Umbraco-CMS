import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type {
	CreateUserGroupRequestModel,
	UpdateUserGroupRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User Group that fetches data from the server
 * @class UmbUserGroupServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupServerDataSource implements UmbDetailDataSource<UmbUserGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserGroupServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbUserGroupServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new User Group scaffold
	 * @param {(string | null)} parentUnique
	 * @returns { CreateUserGroupRequestModel }
	 * @memberof UmbUserGroupServerDataSource
	 */
	async createScaffold() {
		const data: UmbUserGroupDetailModel = {
			alias: '',
			aliasCanBeChanged: true,
			documentRootAccess: false,
			documentStartNode: null,
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			fallbackPermissions: [],
			hasAccessToAllLanguages: false,
			icon: 'icon-users',
			isDeletable: true,
			languages: [],
			mediaRootAccess: false,
			mediaStartNode: null,
			name: '',
			permissions: [],
			sections: [],
			unique: UmbId.new(),
		};

		return { data };
	}

	/**
	 * Fetches a User Group with the given id from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(this.#host, UserGroupService.getUserGroupById({ path: { id: unique } }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const userGroup: UmbUserGroupDetailModel = {
			alias: data.alias,
			documentRootAccess: data.documentRootAccess,
			documentStartNode: data.documentStartNode ? { unique: data.documentStartNode.id } : null,
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			fallbackPermissions: data.fallbackPermissions,
			hasAccessToAllLanguages: data.hasAccessToAllLanguages,
			icon: data.icon || null,
			isDeletable: data.isDeletable,
			aliasCanBeChanged: data.aliasCanBeChanged,
			languages: data.languages,
			mediaRootAccess: data.mediaRootAccess,
			mediaStartNode: data.mediaStartNode ? { unique: data.mediaStartNode.id } : null,
			name: data.name,
			permissions: data.permissions,
			sections: data.sections,
			unique: data.id,
		};

		return { data: userGroup };
	}

	/**
	 * Inserts a new User Group on the server
	 * @param {UmbUserGroupDetailModel} model
	 * @returns {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async create(model: UmbUserGroupDetailModel) {
		if (!model) throw new Error('User Group is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateUserGroupRequestModel = {
			alias: model.alias,
			documentRootAccess: model.documentRootAccess,
			documentStartNode: model.documentStartNode ? { id: model.documentStartNode.unique } : null,
			fallbackPermissions: model.fallbackPermissions,
			hasAccessToAllLanguages: model.hasAccessToAllLanguages,
			icon: model.icon,
			languages: model.languages,
			mediaRootAccess: model.mediaRootAccess,
			mediaStartNode: model.mediaStartNode ? { id: model.mediaStartNode.unique } : null,
			name: model.name,
			permissions: model.permissions,
			sections: model.sections,
		};

		const { data, error } = await tryExecute(
			this.#host,
			UserGroupService.postUserGroup({
				body,
			}),
		);

		if (data) {
			return this.read(data as never);
		}

		return { error };
	}

	/**
	 * Updates a UserGroup on the server
	 * @param {UmbUserGroupDetailModel} UserGroup
	 * @param model
	 * @returns {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async update(model: UmbUserGroupDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateUserGroupRequestModel = {
			alias: model.alias,
			documentRootAccess: model.documentRootAccess,
			documentStartNode: model.documentStartNode ? { id: model.documentStartNode.unique } : null,
			fallbackPermissions: model.fallbackPermissions,
			hasAccessToAllLanguages: model.hasAccessToAllLanguages,
			icon: model.icon,
			languages: model.languages,
			mediaRootAccess: model.mediaRootAccess,
			mediaStartNode: model.mediaStartNode ? { id: model.mediaStartNode.unique } : null,
			name: model.name,
			permissions: model.permissions,
			sections: model.sections,
		};

		const { error } = await tryExecute(
			this.#host,
			UserGroupService.putUserGroupById({
				path: { id: model.unique },
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a User Group on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			UserGroupService.deleteUserGroupById({
				path: { id: unique },
			}),
		);
	}
}
