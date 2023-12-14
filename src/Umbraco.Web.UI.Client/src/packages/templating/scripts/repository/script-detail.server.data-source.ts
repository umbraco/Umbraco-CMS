import { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer } from '../../utils/server-path-unique-serializer.js';
import {
	CreateScriptRequestModel,
	ScriptResource,
	UpdateTextFileViewModelBaseModel,
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
		if (!script.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateScriptRequestModel = {
			parentPath: script.parentUnique,
			name: script.name,
			content: script.content,
		};

		const { error: createError } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.postScript({
				requestBody,
			}),
		);

		if (createError) {
			return { error: createError };
		}

		// We have to fetch the data again. The server can have modified the data after creation
		return this.read(script.unique);
	}

	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const serverPath = this.#serverPathUniqueSerializer.toServerPath(unique);

		const { data, error } = await tryExecuteAndNotify(this.#host, ScriptResource.getScript({ path: serverPath }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const script: UmbScriptDetailModel = {
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			unique: this.#serverPathUniqueSerializer.toUnique(data.path),
			parentUnique: this.#serverPathUniqueSerializer.toParentUnique(data.path),
			path: data.path,
			name: data.name,
			content: data.content,
		};

		return { data: script };
	}

	async update(data: UmbScriptDetailModel) {
		if (!data.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateTextFileViewModelBaseModel = {
			existingPath: this.#serverPathUniqueSerializer.toServerPath(data.unique),
			name: data.name,
			content: data.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.putScript({
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		// We have to fetch the data type again. The server can have modified the data after update
		return this.read(data.unique);
	}

	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			ScriptResource.deleteScript({
				path: this.#serverPathUniqueSerializer.toServerPath(unique),
			}),
		);
	}
}
