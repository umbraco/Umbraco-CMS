import type { UmbDataTypeDetailModel, UmbDataTypePropertyModel } from '../../types.js';
import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { CreateDataTypeRequestModel, DataTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Data Type that fetches data from the server
 * @export
 * @class UmbDataTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeServerDataSource implements UmbDetailDataSource<UmbDataTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Data Type scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateDataTypeRequestModel }
	 * @memberof UmbDataTypeServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbDataTypeDetailModel = {
			entityType: UMB_DATA_TYPE_ENTITY_TYPE,
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
	 * Fetches a Data Type with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, DataTypeResource.getDataTypeById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const dataType: UmbDataTypeDetailModel = {
			entityType: UMB_DATA_TYPE_ENTITY_TYPE,
			unique: data.id,
			parentUnique: data.parent ? data.parent.id : null,
			name: data.name,
			editorAlias: data.editorAlias,
			editorUiAlias: data.editorUiAlias || null,
			values: data.values as Array<UmbDataTypePropertyModel>,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {UmbDataTypeDetailModel} model
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async create(model: UmbDataTypeDetailModel) {
		if (!model) throw new Error('Data Type is missing');
		if (!model.unique) throw new Error('Data Type unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateDataTypeRequestModel = {
			id: model.unique,
			parentId: model.parentUnique,
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataType({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a DataType on the server
	 * @param {UmbDataTypeDetailModel} DataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async update(model: UmbDataTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.editorAlias) throw new Error('Property Editor Alias is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: DataTypeModelBaseModel = {
			name: model.name,
			editorAlias: model.editorAlias,
			editorUiAlias: model.editorUiAlias,
			values: model.values,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeById({
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
	 * Deletes a Data Type on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeById({
				id: unique,
			}),
		);
	}
}
