import { UmbPartialViewDetailServerDataSource } from '../../repository/partial-view-detail.server.data-source.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PartialViewService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

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
	 * @returns {*}
	 * @memberof UmbRenamePartialViewServerDataSource
	 */
	async rename(unique: string, name: string) {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const body: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.cshtml'),
		};

		const { data, error } = await tryExecute(
			this.#host,
			PartialViewService.putPartialViewByPathRename({
				path: { path: encodeURIComponent(path) },
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
