import type { UmbUserGroupDetailModel, UmbUserGroupPropertyModel } from '../../types.js';
import { UMB_USER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateUserGroupRequestModel, UserGroupModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UserGroupResource } from '@umbraco-cms/backoffice/backend-api';
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
	async createScaffold(parentUnique: string | null) {
		const data: UmbUserGroupDetailModel = {
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			unique: UmbId.new(),
			parentUnique,
			name: '',
			editorAlias: undefined,
			editorUiAlias: null,
			values: [],
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
			parentUnique: data.parent ? data.parent.id : null,
			name: data.name,
			editorAlias: data.editorAlias,
			editorUiAlias: data.editorUiAlias || null,
			values: data.values as Array<UmbUserGroupPropertyModel>,
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
		if (!model.unique) throw new Error('User Group unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateUserGroupRequestModel = {
			id: model.unique,
			parent: model.parentUnique ? { id: model.parentUnique } : null,
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
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
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UserGroupModelBaseModel = {
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserGroupResource.putUserGroupById({
				id: model.unique,
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
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
