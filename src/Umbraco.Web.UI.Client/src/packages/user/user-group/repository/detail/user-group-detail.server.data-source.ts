import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import type {
	CreateUserGroupRequestModel,
	UpdateUserGroupRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UserGroupService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbManagementApiDataMapper, type UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for the User Group that fetches data from the server
 * @class UmbUserGroupServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserGroupServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbUserGroupDetailModel>
{
	#dataMapper = new UmbManagementApiDataMapper(this);

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

		const { data, error } = await tryExecuteAndNotify(this, UserGroupService.getUserGroupById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		const permissionDataPromises = data.permissions.map(async (item) => {
			return this.#dataMapper.map({
				forDataModel: item.$type,
				data: item,
				fallback: async () => {
					return {
						...item,
						permissionType: 'unknown',
					};
				},
			});
		});

		const permissions = await Promise.all(permissionDataPromises);

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
			permissions,
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

		const permissionDataPromises = model.permissions.map(async (item) => {
			return this.#dataMapper.map({
				forDataModel: item.permissionType,
				data: item,
				fallback: async () => item,
			});
		});

		const permissions = await Promise.all(permissionDataPromises);

		// TODO: make data mapper to prevent errors
		const requestBody: CreateUserGroupRequestModel = {
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
			permissions,
			sections: model.sections,
		};

		const { data, error } = await tryExecuteAndNotify(
			this,
			UserGroupService.postUserGroup({
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
	 * @param model
	 * @returns {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async update(model: UmbUserGroupDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		const permissionDataPromises = model.permissions.map(async (item) => {
			return this.#dataMapper.map({
				forDataModel: item.permissionType,
				data: item,
				fallback: async () => {
					return {
						...item,
					};
				},
			});
		});

		const permissions = await Promise.all(permissionDataPromises);

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateUserGroupRequestModel = {
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
			permissions,
			sections: model.sections,
		};

		const { error } = await tryExecuteAndNotify(
			this,
			UserGroupService.putUserGroupById({
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
	 * @returns {*}
	 * @memberof UmbUserGroupServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this,
			UserGroupService.deleteUserGroupById({
				id: unique,
			}),
		);
	}
}
