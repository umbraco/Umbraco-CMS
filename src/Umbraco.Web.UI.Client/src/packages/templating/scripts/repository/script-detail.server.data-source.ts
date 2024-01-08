import { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '../../utils/index.js';
import {
	CreateScriptRequestModel,
	ScriptResource,
	UpdateScriptRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbScriptDetailServerDataSource implements UmbDetailDataSource<UmbScriptDetailModel> {
	#host: UmbControllerHost;
	#serverPathUniqueSerializer = new UmbServerPathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async createScaffold(parentUnique: string | null) {
		const data: UmbScriptDetailModel = {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			parentUnique,
			unique: '',
			path: '',
			name: '',
			content: '',
		};

		return { data };
	}

	async create(script: UmbScriptDetailModel) {
		if (!script) throw new Error('Data is missing');
		if (!script.parentUnique === undefined) throw new Error('Parent Unique is missing');
		if (!script.name) throw new Error('Name is missing');

		const parentPath = this.#serverPathUniqueSerializer.toServerPath(script.parentUnique);

		// TODO: make data mapper to prevent errors
		const requestBody: CreateScriptRequestModel = {
			parent: {
				path: parentPath,
			},
			name: appendFileExtensionIfNeeded(script.name, '.js'),
			content: script.content,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.postScript({
				requestBody,
			}),
		);

		if (data) {
			const createdScriptUnique = this.#serverPathUniqueSerializer.toUnique(data);
			return this.read(createdScriptUnique);
		}

		return { error };
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);

		const { data, error } = await tryExecuteAndNotify(this.#host, ScriptResource.getScriptByPath({ path }));

		if (error || !data) {
			return { error };
		}

		const script: UmbScriptDetailModel = {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			unique: this.#serverPathUniqueSerializer.toUnique(data.path),
			parentUnique: data.parent ? this.#serverPathUniqueSerializer.toUnique(data.parent.path) : null,
			path: data.path,
			name: data.name,
			content: data.content,
		};

		return { data: script };
	}

	async update(data: UmbScriptDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(data.unique);

		const requestBody: UpdateScriptRequestModel = {
			content: data.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.putScriptByPath({
				path,
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

		return tryExecuteAndNotify(
			this.#host,
			ScriptResource.deleteScriptByPath({
				path,
			}),
		);
	}
}
