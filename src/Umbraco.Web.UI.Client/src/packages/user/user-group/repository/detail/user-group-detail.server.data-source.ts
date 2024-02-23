import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type {
	CreateUserGroupRequestModel,
	UpdateUserGroupRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserGroupResource } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User Group that fetches data from the server
 * @export
 * @class UmbUserGroupServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupServerDataSource implements UmbDetailDataSource<UmbUserGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserGroupServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserGroupServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new User Group scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateUserGroupRequestModel }
	 * @memberof UmbUserGroupServerDataSource
	 */
	async createScaffold() {
		const data: UmbUserGroupDetailModel = {
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			unique: UmbId.new(),
			isSystemGroup: false,
			name: '',
			icon: null,
			sections: [],
			languages: [],
			hasAccessToAllLanguages: false,
			documentStartNode: null,
			documentRootAccess: false,
			mediaStartNode: null,
			mediaRootAccess: false,
			permissions: [],
		};

		return { data };
	}

	/**
	 * Fetches a User Group with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, UserGroupResource.getUserGroupById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const userGroup: UmbUserGroupDetailModel = {
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			unique: data.id,
			isSystemGroup: data.isSystemGroup,
			name: data.name,
			icon: data.icon || null,
			sections: data.sections,
			languages: data.languages,
			hasAccessToAllLanguages: data.hasAccessToAllLanguages,
			documentStartNode: data.documentStartNode ? { unique: data.documentStartNode.id } : null,
			documentRootAccess: data.documentRootAccess,
			mediaStartNode: data.mediaStartNode ? { unique: data.mediaStartNode.id } : null,
			mediaRootAccess: data.mediaRootAccess,
			permissions: data.permissions,
		};

		return { data: userGroup };
	}

	/**
	 * Inserts a new User Group on the server
	 * @param {UmbUserGroupDetailModel} model
	 * @return {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async create(model: UmbUserGroupDetailModel) {
		if (!model) throw new Error('User Group is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateUserGroupRequestModel = {
			name: model.name,
			icon: model.icon,
			sections: model.sections,
			languages: model.languages,
			hasAccessToAllLanguages: model.hasAccessToAllLanguages,
			documentStartNode: model.documentStartNode ? { id: model.documentStartNode.unique } : null,
			documentRootAccess: model.documentRootAccess,
			mediaStartNode: model.mediaStartNode ? { id: model.mediaStartNode.unique } : null,
			mediaRootAccess: model.mediaRootAccess,
			permissions: model.permissions,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserGroupResource.postUserGroup({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a UserGroup on the server
	 * @param {UmbUserGroupDetailModel} UserGroup
	 * @return {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async update(model: UmbUserGroupDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateUserGroupRequestModel = {
			name: model.name,
			icon: model.icon,
			sections: model.sections,
			languages: model.languages,
			hasAccessToAllLanguages: model.hasAccessToAllLanguages,
			documentStartNode: model.documentStartNode ? { id: model.documentStartNode.unique } : null,
			documentRootAccess: model.documentRootAccess,
			mediaStartNode: model.mediaStartNode ? { id: model.mediaStartNode.unique } : null,
			mediaRootAccess: model.mediaRootAccess,
			fallbackPermissions: model.fallbackPermissions,
			permissions: model.permissions,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			UserGroupResource.putUserGroupById({
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
	 * Deletes a User Group on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserGroupResource.deleteUserGroupById({
				id: unique,
			}),
		);
	}
}
