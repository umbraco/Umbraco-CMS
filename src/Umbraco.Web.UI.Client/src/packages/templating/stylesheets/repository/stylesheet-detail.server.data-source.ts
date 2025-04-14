import type { UmbStylesheetDetailModel } from '../types.js';
import { UMB_STYLESHEET_ENTITY_TYPE } from '../entity.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type {
	CreateStylesheetRequestModel,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StylesheetService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbStylesheetDetailServerDataSource implements UmbDetailDataSource<UmbStylesheetDetailModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(preset: Partial<UmbStylesheetDetailModel> = {}) {
		const data: UmbStylesheetDetailModel = {
			entityType: UMB_STYLESHEET_ENTITY_TYPE,
			unique: '',
			name: '',
			content: '',
			...preset,
		};

		return { data };
	}

	async create(model: UmbStylesheetDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Data is missing');
		if (!model.name) throw new Error('Name is missing');

		const parentPath = this.#serverFilePathUniqueSerializer.toServerPath(parentUnique);

		// TODO: make data mapper to prevent errors
		const body: CreateStylesheetRequestModel = {
			parent: parentPath ? { path: parentPath } : null,
			name: appendFileExtensionIfNeeded(model.name, '.css'),
			content: model.content,
		};

		const { data, error } = await tryExecute(
			this.#host,
			StylesheetService.postStylesheet({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverFilePathUniqueSerializer.toUnique(newPath);
			return this.read(newPathUnique);
		}

		return { error };
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const { data, error } = await tryExecute(
			this.#host,
			StylesheetService.getStylesheetByPath({ path: { path: encodeURIComponent(path) } }),
		);

		if (error || !data) {
			return { error };
		}

		const stylesheet: UmbStylesheetDetailModel = {
			entityType: UMB_STYLESHEET_ENTITY_TYPE,
			unique: this.#serverFilePathUniqueSerializer.toUnique(data.path),
			name: data.name,
			content: data.content,
		};

		return { data: stylesheet };
	}

	async update(model: UmbStylesheetDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(model.unique);
		if (!path) throw new Error('Path is missing');

		const body: UpdateStylesheetRequestModel = {
			content: model.content,
		};

		const { error } = await tryExecute(
			this.#host,
			StylesheetService.putStylesheetByPath({
				path: { path: encodeURIComponent(path) },
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		return tryExecute(
			this.#host,
			StylesheetService.deleteStylesheetByPath({
				path: { path: encodeURIComponent(path) },
			}),
		);
	}
}
