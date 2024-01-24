import { UmbStylesheetDetailServerDataSource } from '../../repository/stylesheet-detail.server.data-source.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import { RenameStylesheetRequestModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbRenameStylesheetServerDataSource {
	#host: UmbControllerHost;
	#detailDataSource: UmbStylesheetDetailServerDataSource;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#detailDataSource = new UmbStylesheetDetailServerDataSource(this.#host);
	}

	/**
	 * Rename Stylesheet
	 * @param {string} unique
	 * @param {string} name
	 * @return {*}
	 * @memberof UmbRenameStylesheetServerDataSource
	 */
	async rename(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const requestBody: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.css'),
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			StylesheetResource.putStylesheetByPathRename({
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
