import { UmbScriptDetailServerDataSource } from '../../repository/script-detail.server.data-source.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ScriptService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbRenameScriptServerDataSource {
	#host: UmbControllerHost;
	#detailDataSource: UmbScriptDetailServerDataSource;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#detailDataSource = new UmbScriptDetailServerDataSource(this.#host);
	}

	/**
	 * Rename Script
	 * @param {string} unique
	 * @param {string} name
	 * @returns {*}
	 * @memberof UmbRenameScriptServerDataSource
	 */
	async rename(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const requestBody: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.js'),
		};

		const { data, error } = await tryExecute(
			this.#host,
			ScriptService.putScriptByPathRename({
				path: encodeURIComponent(path),
				requestBody,
			}),
		);

		if (data) {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverFilePathUniqueSerializer.toUnique(newPath);
			return this.#detailDataSource.read(newPathUnique);
		}

		return { error };
	}
}
