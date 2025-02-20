import { UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import type { CreatePartialViewFolderRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PartialViewService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Partial View folders that fetches data from the server
 * @class UmbPartialViewFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbPartialViewFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * Creates an instance of UmbPartialViewFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbPartialViewFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Partial View folder
	 * @param {Partial<UmbFolderModel>} [preset]
	 * @returns {*}
	 * @memberof UmbPartialViewFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>) {
		const scaffold: UmbFolderModel = {
			entityType: UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Partial View folder from the server
	 * @param {string} unique
	 * @returns {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbPartialViewFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot read partial view folder without a path');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewService.getPartialViewFolderByPath({
				path: encodeURIComponent(path),
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE,
				unique: this.#serverFilePathUniqueSerializer.toUnique(data.path),
				parentUnique: data.parent ? this.#serverFilePathUniqueSerializer.toUnique(data.parent.path) : null,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Partial View folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {UmbDataSourceResponse<UmbFolderModel>}
	 * @memberof UmbPartialViewFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(parentUnique);

		const requestBody: CreatePartialViewFolderRequestModel = {
			parent: parentPath ? { path: parentPath } : null,
			name: model.name,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewService.postPartialViewFolder({
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
	 * Deletes a Partial View folder on the server
	 * @param {string} unique
	 * @returns {UmbDataSourceErrorResponse}
	 * @memberof UmbPartialViewServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Cannot delete partial view folder without a path');

		return tryExecuteAndNotify(
			this.#host,
			PartialViewService.deletePartialViewFolderByPath({
				path: encodeURIComponent(path),
			}),
		);
	}

	async update(): Promise<any> {
		throw new Error('Updating is not supported');
	}
}
