import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import { UmbPartialViewDetailServerDataSource } from '../../repository/partial-view-detail.server.data-source.js';
import { RenameStylesheetRequestModel, PartialViewResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbRenamePartialViewServerDataSource {
	#host: UmbControllerHost;
	#detailDataSource: UmbPartialViewDetailServerDataSource;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#detailDataSource = new UmbPartialViewDetailServerDataSource(this.#host);
	}

	/**
	 * Rename Partial View
	 * @param {string} unique
	 * @param {string} name
	 * @return {*}
	 * @memberof UmbRenamePartialViewServerDataSource
	 */
	async rename(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const requestBody: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.cshtml'),
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			PartialViewResource.putPartialViewByPathRename({
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
