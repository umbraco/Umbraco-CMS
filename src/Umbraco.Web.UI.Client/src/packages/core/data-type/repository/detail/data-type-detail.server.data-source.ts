import { UmbDataTypeDetailModel } from '../../types.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DataTypeResource,
	DataTypeModelBaseModel,
	CreateDataTypeRequestModel,
	UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Data Type that fetches data from the server
 * @export
 * @class UmbDataTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeServerDataSource
	implements UmbDataSource<CreateDataTypeRequestModel, any, UpdateDataTypeRequestModel, UmbDataTypeDetailModel>
{
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
	async createScaffold(parentId?: string | null) {
		const data: CreateDataTypeRequestModel = {
			id: UmbId.new(),
			parentId,
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
	async read(id: string) {
		if (!id) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeById({
				id: id,
			}),
		);
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {Document} dataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async create(dataType: CreateDataTypeRequestModel) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.id) throw new Error('Data Type id is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataType({
				requestBody: dataType,
			}),
		);
	}

	/**
	 * Updates a DataType on the server
	 * @param {UmbDataTypeDetailModel} DataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async update(id: string, data: DataTypeModelBaseModel) {
		if (!id) throw new Error('Key is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeById({
				id: id,
				requestBody: data,
			}),
		);
	}

	/**
	 * Deletes a Data Type on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) throw new Error('Key is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeById({
				id: id,
			}),
		);
	}
}
