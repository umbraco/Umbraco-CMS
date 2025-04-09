import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type { CreateScriptRequestModel, UpdateScriptRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ScriptService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbScriptDetailServerDataSource implements UmbDetailDataSource<UmbScriptDetailModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(preset: Partial<UmbScriptDetailModel> = {}) {
		const data: UmbScriptDetailModel = {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			unique: '',
			name: '',
			content: '',
			...preset,
		};

		return { data };
	}

	async create(model: UmbScriptDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Data is missing');
		if (!model.name) throw new Error('Name is missing');

		const parentPath = this.#serverFilePathUniqueSerializer.toServerPath(parentUnique);

		// TODO: make data mapper to prevent errors
		const body: CreateScriptRequestModel = {
			parent: parentPath ? { path: parentPath } : null,
			name: appendFileExtensionIfNeeded(model.name, '.js'),
			content: model.content,
		};

		const { data, error } = await tryExecute(
			this.#host,
			ScriptService.postScript({
				body,
			}),
		);

		if (data) {
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
			ScriptService.getScriptByPath({ path: encodeURIComponent(path) }),
		);

		if (error || !data) {
			return { error };
		}

		const script: UmbScriptDetailModel = {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			unique: this.#serverFilePathUniqueSerializer.toUnique(data.path),
			name: data.name,
			content: data.content,
		};

		return { data: script };
	}

	async update(model: UmbScriptDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(model.unique);
		if (!path) throw new Error('Path is missing');

		const body: UpdateScriptRequestModel = {
			content: model.content,
		};

		const { error } = await tryExecute(
			this.#host,
			ScriptService.putScriptByPath({
				path: encodeURIComponent(path),
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
			ScriptService.deleteScriptByPath({
				path: encodeURIComponent(path),
			}),
		);
	}
}
