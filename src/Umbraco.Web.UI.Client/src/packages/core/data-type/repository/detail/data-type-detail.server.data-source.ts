import { UmbDataTypeDetailModel } from '../../types.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import {
	CreateDataTypeRequestModel,
	DataTypeModelBaseModel,
	DataTypeResource,
} from '@umbraco-cms/backoffice/backend-api';
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
	 * @param {(string | null)} parentId
	 * @return { CreateDataTypeRequestModel }
	 * @memberof UmbDataTypeServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbDataTypeDetailModel = {
			type: 'data-type',
			unique: UmbId.new(),
			parentUnique,
			name: '',
			propertyEditorAlias: undefined,
			propertyEditorUiAlias: null,
			values: [],
		};

		return { data };
	}

	/**
	 * Fetches a Data Type with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, DataTypeResource.getDataTypeById({ id: unique }));

		if (error) {
			return { error };
		}

		// map to client model
		const dataType = {
			entityType: 'data-type',
			unique: data.id,
			parentUnique: data.parentId,
			name: data.name,
			propertyEditorAlias: data.propertyEditorAlias,
			propertyEditorUiAlias: data.propertyEditorAlias,
			values: data.values,
		};

		return { data: dataType };
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {UmbDataTypeDetailModel} dataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async create(dataType: UmbDataTypeDetailModel) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.unique) throw new Error('Data Type id is missing');

		// map to server model
		const requestBody: CreateDataTypeRequestModel = {
			id: dataType.unique,
			parentId: dataType.parentUnique,
			name: dataType.name,
			propertyEditorAlias: dataType.propertyEditorAlias,
			propertyEditorUiAlias: dataType.propertyEditorUiAlias,
			values: dataType.values,
		};

		const { error: createError } = await tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataType({
				requestBody,
			}),
		);

		if (createError) {
			return { error: createError };
		}

		// we have to fetch the data type again. The server can have modified the data after creation
		return this.read(dataType.unique);
	}

	/**
	 * Updates a DataType on the server
	 * @param {UmbDataTypeDetailModel} DataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async update(data: UmbDataTypeDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		const requestBody: DataTypeModelBaseModel = {
			name: data.name,
			propertyEditorAlias: data.propertyEditorAlias,
			propertyEditorUiAlias: data.propertyEditorUiAlias,
			values: data.values,
		};

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeById({
				id: data.unique,
				requestBody,
			}),
		);
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
