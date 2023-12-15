import { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '../../utils/index.js';
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
		if (!script.parentUnique === undefined) throw new Error('Parent Unique is missing');
		if (!script.name) throw new Error('Name is missing');

		const parentPath = this.#serverPathUniqueSerializer.toServerPath(script.parentUnique);

		// TODO: make data mapper to prevent errors
		const requestBody: CreateScriptRequestModel = {
			parentPath,
			name: appendFileExtensionIfNeeded(script.name, '.js'),
			content: script.content,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.postScript({
				requestBody,
			}),
		);

		if (error) {
			return { error };
		}

		// We have to fetch the data again. The server can have modified the data after creation
		// TODO: revisit when location header is added
		const createdScriptUnique = this.#serverPathUniqueSerializer.toUnique(parentPath + '/' + requestBody.name);
		return this.read(createdScriptUnique);
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

		const existingPath = this.#serverPathUniqueSerializer.toServerPath(data.unique);

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateTextFileViewModelBaseModel = {
			existingPath,
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
			ScriptResource.deleteScript({
				path,
			}),
		);
	}
}
