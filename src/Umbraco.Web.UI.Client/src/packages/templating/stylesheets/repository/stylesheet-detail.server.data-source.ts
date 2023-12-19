import { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '../../utils/index.js';
import {
	CreateStylesheetRequestModel,
	StylesheetResource,
	UpdateTextFileViewModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbStylesheetDetailServerDataSource implements UmbDetailDataSource<UmbScriptDetailModel> {
	#host: UmbControllerHost;
	#serverPathUniqueSerializer = new UmbServerPathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(parentUnique: string | null) {
		const data: UmbStylesheetDetailModel = {
			entityType: UMB_STYLESHEET_ENTITY_TYPE,
			parentUnique,
			unique: '',
			path: '',
			name: '',
			content: '',
		};

		return { data };
	}

	async create(stylesheet: UmbStylesheetDetailModel) {
		if (!stylesheet) throw new Error('Data is missing');
		if (!stylesheet.parentUnique === undefined) throw new Error('Parent Unique is missing');
		if (!stylesheet.name) throw new Error('Name is missing');

		const parentPath = this.#serverPathUniqueSerializer.toServerPath(stylesheet.parentUnique);

		// TODO: make data mapper to prevent errors
		const requestBody: CreateStylesheetRequestModel = {
			parentPath,
			name: appendFileExtensionIfNeeded(stylesheet.name, '.css'),
			content: stylesheet.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.postStylesheet({
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		// We have to fetch the data again. The server can have modified the data after creation
		// TODO: revisit when location header is added
		const createdStylesheetPath = parentPath ? parentPath + '/' + requestBody.name : requestBody.name;
		const createdStylesheetUnique = this.#serverPathUniqueSerializer.toUnique(createdStylesheetPath);

		return this.read(createdStylesheetUnique);
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const serverPath = this.#serverPathUniqueSerializer.toServerPath(unique);

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getStylesheet({ path: serverPath }),
		);

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const stylesheet: UmbStylesheetDetailModel = {
			entityType: UMB_STYLESHEET_ENTITY_TYPE,
			unique: this.#serverPathUniqueSerializer.toUnique(data.path),
			parentUnique: this.#serverPathUniqueSerializer.toParentUnique(data.path),
			path: data.path,
			name: data.name,
			content: data.content,
		};

		return { data: stylesheet };
	}

	async update(data: UmbStylesheetDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		const existingPath = this.#serverPathUniqueSerializer.toServerPath(data.unique);

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateTextFileViewModelBaseModel = {
			existingPath,
			name: data.name,
			content: data.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.putStylesheet({
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		// TODO: should we get this as part of the PUT response?
		const parentPath = this.#serverPathUniqueSerializer.toServerPath(data.parentUnique);
		const newFilePath = parentPath + '/' + requestBody.name;
		const newPathUnique = this.#serverPathUniqueSerializer.toUnique(newFilePath);
		// We have to fetch the data type again. The server can have modified the data after update
		return this.read(newPathUnique);
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);

		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.deleteStylesheet({
				path,
			}),
		);
	}
}
