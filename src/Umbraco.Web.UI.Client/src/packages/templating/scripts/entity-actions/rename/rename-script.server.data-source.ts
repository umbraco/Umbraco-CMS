import { UmbServerPathUniqueSerializer, appendFileExtensionIfNeeded } from '../../../utils/index.js';
import { UmbScriptDetailServerDataSource } from '../../repository/script-detail.server.data-source.js';
import { RenameStylesheetRequestModel, ScriptResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbRenameScriptServerDataSource {
	#host: UmbControllerHost;
	#detailDataSource: UmbScriptDetailServerDataSource;
	#serverPathUniqueSerializer = new UmbServerPathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#detailDataSource = new UmbScriptDetailServerDataSource(this.#host);
	}

	/**
	 * Rename Script
	 * @param {string} unique
	 * @param {string} name
	 * @return {*}
	 * @memberof UmbRenameScriptServerDataSource
	 */
	async rename(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverPathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const requestBody: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.js'),
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			ScriptResource.putScriptByPathRename({
				path: encodeURIComponent(path),
				requestBody,
			}),
		);

		if (data) {
			const newPath = decodeURIComponent(data);
			const newPathUnique = this.#serverPathUniqueSerializer.toUnique(newPath);
			return this.#detailDataSource.read(newPathUnique);
		}

		return { error };
	}
}
