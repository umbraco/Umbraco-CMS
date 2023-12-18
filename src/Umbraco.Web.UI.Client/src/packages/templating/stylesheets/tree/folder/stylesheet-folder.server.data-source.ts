import { UmbServerPathUniqueSerializer } from '../../../utils/index.js';
import { UmbCreateFolderModel, UmbFolderDataSource, UmbUpdateFolderModel } from '@umbraco-cms/backoffice/tree';
import { StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
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

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getStylesheetFolder({
				path,
			}),
		);

		if (data) {
			const mappedData = {
				unique: this.#serverPathUniqueSerializer.toUnique(data.path),
				parentUnique: this.#serverPathUniqueSerializer.toParentUnique(data.path),
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

		const requestBody = {
			parentPath,
			name: args.name,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheetFolder({
				requestBody,
			}),
		);

		if (!error) {
			/* TODO: investigate why we don't get the location header as part of data, 
			so we don't have to construct the path ourselves */
			const newPath = parentPath ? `${parentPath}/${args.name}` : args.name;
			return this.read(newPath);
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

		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.deleteStylesheetFolder({
				path,
			}),
		);
	}

	async update(args: UmbUpdateFolderModel): Promise<any> {
		throw new Error('Updating is not supported');
	}
}
