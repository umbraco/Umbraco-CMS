import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { DocumentBlueprintService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for a Document Blueprint folder that fetches data from the server
 * @class UmbDocumentBlueprintFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentBlueprintFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentBlueprintFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Document Blueprint folder
	 * @param {Partial<UmbFolderModel>} [preset]
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>) {
		const scaffold: UmbFolderModel = {
			entityType: UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Document Blueprint folder from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.getDocumentBlueprintFolderById({
				id: unique,
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
				unique: data.id,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Document Blueprint folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Model is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const body = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.postDocumentBlueprintFolder({
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Updates a Document Blueprint folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintFolderServerDataSource
	 */
	async update(model: UmbFolderModel) {
		if (!model) throw new Error('Model is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Folder name is missing');

		const { error } = await tryExecute(
			this.#host,
			DocumentBlueprintService.putDocumentBlueprintFolderById({
				id: model.unique,
				body: { name: model.name },
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Document Blueprint folder on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentBlueprintServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(
			this.#host,
			DocumentBlueprintService.deleteDocumentBlueprintFolderById({
				id: unique,
			}),
		);
	}
}
