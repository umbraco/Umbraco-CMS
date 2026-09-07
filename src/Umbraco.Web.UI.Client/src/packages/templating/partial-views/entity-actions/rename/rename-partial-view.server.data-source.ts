import { UmbPartialViewDetailServerDataSource } from '../../repository/partial-view-detail.server.data-source.js';
import type { UmbPartialViewDetailModel } from '../../types.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PartialViewService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @param {string} unique - The unique identifier of the partial view to rename
	 * @param {string} name - The new name for the partial view
	 * @returns {UmbDataSourceResponse<UmbPartialViewDetailModel>} The renamed partial view, or an error
	 * @memberof UmbRenamePartialViewServerDataSource
	 */
	async rename(unique: string, name: string): Promise<UmbDataSourceResponse<UmbPartialViewDetailModel>> {
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
