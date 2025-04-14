import { UMB_STYLESHEET_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbStylesheetItemModel } from '../../types.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { StylesheetService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source for stylesheet items that fetches data from the server
 * @class UmbStylesheetItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbStylesheetItemServerDataSource implements UmbItemDataSource<UmbStylesheetItemModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * Creates an instance of UmbStylesheetItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbStylesheetItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given uniques from the server
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbStylesheetItemServerDataSource
	 */
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const paths = uniques
			.map((unique) => {
				const serverPath = this.#serverFilePathUniqueSerializer.toServerPath(unique);
				return serverPath ? serverPath : null;
			})
			.filter((x) => x !== null) as string[];

		const { data, error } = await tryExecute(
			this.#host,
			StylesheetService.getItemStylesheet({
				query: { path: paths },
			}),
		);

		if (data) {
			const items: Array<UmbStylesheetItemModel> = data.map((item) => {
				return {
					entityType: item.isFolder ? UMB_STYLESHEET_FOLDER_ENTITY_TYPE : UMB_STYLESHEET_ENTITY_TYPE,
					unique: this.#serverFilePathUniqueSerializer.toUnique(item.path),
					parentUnique: item.parent ? this.#serverFilePathUniqueSerializer.toUnique(item.parent.path) : null,
					name: item.name,
					isFolder: item.isFolder,
				};
			});

			return { data: items };
		}

		return { error };
	}
}
