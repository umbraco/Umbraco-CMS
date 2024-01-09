import { UmbServerPathUniqueSerializer } from '../../../utils/index.js';
import { UmbCreateFolderModel, UmbFolderDataSource, UmbUpdateFolderModel } from '@umbraco-cms/backoffice/tree';
import { CreateStylesheetFolderRequestModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Stylesheet folders that fetches data from the server
 * @export
 * @class UmbStylesheetFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbStylesheetFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;
	#serverPathUniqueSerializer = new UmbServerPathUniqueSerializer();

	/**
	 * Creates an instance of UmbStylesheetFolderServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbStylesheetFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches a Stylesheet folder from the server
	 * @param {string} unique
	 * @return {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbStylesheetFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot read stylesheet folder without a path');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getStylesheetFolderByPath({
				path: encodeURIComponent(path),
			}),
		);

		if (data) {
			const mappedData = {
				unique: this.#serverPathUniqueSerializer.toUnique(data.path),
				parentUnique: data.parent ? this.#serverPathUniqueSerializer.toUnique(data.parent.path) : null,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Stylesheet folder on the server
	 * @param {UmbCreateFolderModel} args
	 * @return {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbStylesheetFolderServerDataSource
	 */
	async create(args: UmbCreateFolderModel) {
		if (args.parentUnique === undefined) throw new Error('Parent unique is missing');
		if (!args.name) throw new Error('Name is missing');

		const parentPath = new UmbServerPathUniqueSerializer().toServerPath(args.parentUnique);

		const requestBody: CreateStylesheetFolderRequestModel = {
			parent: parentPath ? { path: parentPath } : null,
			name: args.name,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheetFolder({
				requestBody,
			}),
		);

		if (data) {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverPathUniqueSerializer.toUnique(newPath);
			return this.read(newPathUnique);
		}

		return { error };
	}

	/**
	 * Deletes a Stylesheet folder on the server
	 * @param {string} unique
	 * @return {UmbDataSourceErrorResponse}
	 * @memberof UmbStylesheetServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot delete stylesheet folder without a path');

		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.deleteStylesheetFolderByPath({
				path: encodeURIComponent(path),
			}),
		);
	}

	async update(args: UmbUpdateFolderModel): Promise<any> {
		throw new Error('Updating is not supported');
	}
}
