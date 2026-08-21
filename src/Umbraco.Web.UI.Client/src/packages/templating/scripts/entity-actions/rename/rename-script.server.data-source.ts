import { UmbScriptDetailServerDataSource } from '../../repository/script-detail.server.data-source.js';
import type { UmbScriptDetailModel } from '../../types.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ScriptService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @param {string} unique - The unique identifier of the script to rename
	 * @param {string} name - The new name for the script
	 * @returns {UmbDataSourceResponse<UmbScriptDetailModel>} The renamed script, or an error
	 * @memberof UmbRenameScriptServerDataSource
	 */
	async rename(unique: string, name: string): Promise<UmbDataSourceResponse<UmbScriptDetailModel>> {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const body: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.js'),
		};

		const { data, error } = await tryExecute(
			this.#host,
			ScriptService.putScriptByPathRename({
				path: { path },
				body,
			}),
		);

		if (data && typeof data === 'string') {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverFilePathUniqueSerializer.toUnique(newPath);
			return this.#detailDataSource.read(newPathUnique);
		}

		return { error };
	}
}
