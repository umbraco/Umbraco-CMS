import { UmbStylesheetDetailServerDataSource } from '../../repository/stylesheet-detail.server.data-source.js';
import type { UmbStylesheetDetailModel } from '../../types.js';
import {
	UmbServerFilePathUniqueSerializer,
	appendFileExtensionIfNeeded,
} from '@umbraco-cms/backoffice/server-file-system';
import type { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { StylesheetService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

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
	 * @param {string} unique - The unique identifier of the Stylesheet
	 * @param {string} name - The new name of the Stylesheet
	 * @returns {UmbDataSourceResponse<UmbStylesheetDetailModel>} The renamed Stylesheet
	 * @memberof UmbRenameStylesheetServerDataSource
	 */
	async rename(unique: string, name: string): Promise<UmbDataSourceResponse<UmbStylesheetDetailModel>> {
		if (!unique) throw new Error('Unique is missing');
		if (!name) throw new Error('Name is missing');

		const path = this.#serverFilePathUniqueSerializer.toServerPath(unique);
		if (!path) throw new Error('Path is missing');

		const body: RenameStylesheetRequestModel = {
			name: appendFileExtensionIfNeeded(name, '.css'),
		};

		const { data, error } = await tryExecute(
			this.#host,
			StylesheetService.putStylesheetByPathRename({
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
