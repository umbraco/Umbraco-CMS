import { v4 as uuidv4 } from 'uuid';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	ProblemDetailsModel,
	DataTypeResource,
	DataTypeResponseModel,
	DataTypeModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Data Type that fetches data from the server
 * @export
 * @class UmbDataTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeServerDataSource implements UmbDataSource<DataTypeResponseModel> {
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
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.getDataTypeByKey({
				key,
			})
		);
	}

	/**
	 * Creates a new Data Type scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async createScaffold(parentKey: string | null) {
		const data: DataTypeResponseModel = {
			$type: '',
			parentKey: parentKey,
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
	async insert(dataType: DataTypeResponseModel) {
		if (!dataType.key) {
			const error: ProblemDetailsModel = { title: 'DataType key is missing' };
			return { error };
		}
		const requestBody: DataTypeModelBaseModel = { ...dataType };

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DataTypeResponseModel>(
			this.#host,
			// TODO: avoid this any?..
			tryExecuteAndNotify(
				this.#host,
				DataTypeResource.postDataType({
					requestBody,
				})
			) as any
		);
	}

	/**
	 * Updates a DataType on the server
	 * @param {DataTypeResponseModel} DataType
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	// TODO: Error mistake in this:
	async update(dataType: DataTypeResponseModel) {
		if (!dataType.key) {
			const error: ProblemDetailsModel = { title: 'DataType key is missing' };
			return { error };
		}

		const requestBody: DataTypeModelBaseModel = { ...dataType };

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DataTypeResponseModel>(
			this.#host,
			DataTypeResource.putDataTypeByKey({
				key: dataType.key,
				requestBody,
			})
		);
	}

	/**
	 * Trash a Document on the server
	 * @param {Document} Document
	 * @return {*}
	 * @memberof UmbDataTypeServerDataSource
	 */
	async trash(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'DataType key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DataTypeResponseModel>(
			this.#host,
			DataTypeResource.deleteDataTypeByKey({
				key,
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
		if (!key) {
			const error: ProblemDetailsModel = { title: 'DataType key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<DataTypeResponseModel>(
			this.#host,
			DataTypeResource.deleteDataTypeByKey({
				key,
			})
		);
	}
}
