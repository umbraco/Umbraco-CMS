import { v4 as uuidv4 } from 'uuid';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	DataTypeResource,
	DataTypeResponseModel,
	DataTypeModelBaseModel,
	CreateDataTypeRequestModel,
	UpdateDataTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Data Type that fetches data from the server
 * @export
 * @class UmbDataTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeServerDataSource
	implements
		UmbDataSource<CreateDataTypeRequestModel & { key: string }, UpdateDataTypeRequestModel, DataTypeResponseModel>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDataTypeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Data Type with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async get(key: string) {
		if (!key) throw new Error('Key is missing');
		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeByKey({
				key,
			})
		);
	}

	/**
	 * Creates a new Data Type scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		const data: DataTypeResponseModel = {
			$type: '',
			parentId: parentId,
			key: uuidv4(),
		};

		return { data };
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {Document} dataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async insert(dataType: CreateDataTypeRequestModel & { key: string }) {
		if (!dataType) throw new Error('Data Type is missing');
		if (!dataType.key) throw new Error('Data Type key is missing');

		tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataType({
				requestBody: dataType,
			})
		);
	}

	/**
	 * Updates a DataType on the server
	 * @param {DataTypeResponseModel} DataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async update(key: string, data: DataTypeModelBaseModel) {
		if (!key) throw new Error('Key is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.putDataTypeByKey({
				key,
				requestBody: data,
			})
		);
	}

	/**
	 * Deletes a Data Type on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async delete(key: string) {
		if (!key) throw new Error('Key is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.deleteDataTypeByKey({
				key,
			})
		);
	}
}
