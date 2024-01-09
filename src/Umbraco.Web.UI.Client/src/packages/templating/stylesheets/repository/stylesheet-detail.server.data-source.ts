import { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '../../utils/index.js';
import {
	CreateStylesheetRequestModel,
	StylesheetResource,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbStylesheetDetailServerDataSource implements UmbDetailDataSource<UmbStylesheetDetailModel> {
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
			parent: parentPath ? { path: parentPath } : null,
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

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.getStylesheetByPath({ path: encodeURIComponent(path) }),
		);

		if (error || !data) {
			return { error };
		}

		const stylesheet: UmbStylesheetDetailModel = {
			entityType: UMB_STYLESHEET_ENTITY_TYPE,
			unique: this.#serverPathUniqueSerializer.toUnique(data.path),
			parentUnique: data.parent ? this.#serverPathUniqueSerializer.toUnique(data.parent.path) : null,
			path: data.path,
			name: data.name,
			content: data.content,
		};

		return { data: stylesheet };
	}

	async update(data: UmbStylesheetDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(data.unique);
		if (!path) throw new Error('Path is missing');

		const requestBody: UpdateStylesheetRequestModel = {
			content: data.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.putStylesheetByPath({
				path: encodeURIComponent(path),
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		return this.read(data.unique);
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		return tryExecuteAndNotify(
			this.#host,
			StylesheetResource.deleteStylesheetByPath({
				path: encodeURIComponent(path),
			}),
		);
	}
}
