import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import type { CreateStylesheetFolderRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { StylesheetService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Stylesheet folders that fetches data from the server
 * @class UmbStylesheetFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbStylesheetFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * Creates an instance of UmbStylesheetFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStylesheetFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(preset?: Partial<UmbFolderModel>) {
		const scaffold: UmbFolderModel = {
			entityType: UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Stylesheet folder from the server
	 * @param {string} unique
	 * @returns {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbStylesheetFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot read stylesheet folder without a path');

		const { data, error } = await tryExecute(
			this.#host,
			StylesheetService.getStylesheetFolderByPath({
				path: encodeURIComponent(path),
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_STYLESHEET_FOLDER_ENTITY_TYPE,
				unique: this.#serverFilePathUniqueSerializer.toUnique(data.path),
				parentUnique: data.parent ? this.#serverFilePathUniqueSerializer.toUnique(data.parent.path) : null,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Stylesheet folder on the server
	 * @param {UmbCreateFolderModel} model
	 * @returns {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbStylesheetFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(parentUnique);

		const requestBody: CreateStylesheetFolderRequestModel = {
			parent: parentPath ? { path: parentPath } : null,
			name: model.name,
		};

		const { data, error } = await tryExecute(
			this.#host,
			StylesheetService.postStylesheetFolder({
				requestBody,
			}),
		);

		if (data) {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverFilePathUniqueSerializer.toUnique(newPath);
			return this.read(newPathUnique);
		}

		return { error };
	}

	/**
	 * Deletes a Stylesheet folder on the server
	 * @param {string} unique
	 * @returns {UmbDataSourceErrorResponse}
	 * @memberof UmbStylesheetServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot delete stylesheet folder without a path');

		return tryExecute(
			this.#host,
			StylesheetService.deleteStylesheetFolderByPath({
				path: encodeURIComponent(path),
			}),
		);
	}

	async update(): Promise<any> {
		throw new Error('Updating is not supported');
	}
}
