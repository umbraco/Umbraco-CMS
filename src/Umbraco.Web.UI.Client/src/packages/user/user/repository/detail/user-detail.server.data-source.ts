import type { UmbUserDetailModel, UmbUserPropertyModel } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateUserRequestModel, UserModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the User that fetches data from the server
 * @export
 * @class UmbUserServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbUserServerDataSource implements UmbDetailDataSource<UmbUserDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbUserServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new User scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateUserRequestModel }
	 * @memberof UmbUserServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbUserDetailModel = {
			entityType: UMB_USER_ENTITY_TYPE,
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
	 * Fetches a User with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, UserResource.getUserById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const dataType: UmbUserDetailModel = {
			entityType: UMB_USER_ENTITY_TYPE,
			unique: data.id,
			parentUnique: data.parent ? data.parent.id : null,
			name: data.name,
			editorAlias: data.editorAlias,
			editorUiAlias: data.editorUiAlias || null,
			values: data.values as Array<UmbUserPropertyModel>,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new User on the server
	 * @param {UmbUserDetailModel} model
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	async create(model: UmbUserDetailModel) {
		if (!model) throw new Error('User is missing');
		if (!model.unique) throw new Error('User unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateUserRequestModel = {
			id: model.unique,
			parentId: model.parentUnique,
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserResource.postUser({
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
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	async update(model: UmbUserDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UserModelBaseModel = {
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			UserResource.putUserById({
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
	 * Deletes a User on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbUserServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			UserResource.deleteUserById({
				id: unique,
			}),
		);
	}
}
