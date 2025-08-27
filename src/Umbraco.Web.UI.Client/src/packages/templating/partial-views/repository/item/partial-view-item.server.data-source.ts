import { UMB_PARTIAL_VIEW_ENTITY_TYPE, UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbPartialViewItemModel } from '../../types.js';
import { UmbManagementApiPartialViewItemDataRequestManager } from './partial-view-item.server.request-manager.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for script items that fetches data from the server
 * @class UmbPartialViewItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbPartialViewItemServerDataSource
	extends UmbControllerBase
	implements UmbItemDataSource<UmbPartialViewItemModel>
{
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();
	#itemRequestManager = new UmbManagementApiPartialViewItemDataRequestManager(this);

	/**
	 * Fetches the items for the given uniques from the server
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbPartialViewItemServerDataSource
	 */
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const paths = uniques
			.map((unique) => {
				const serverPath = this.#serverFilePathUniqueSerializer.toServerPath(unique);
				return serverPath ? serverPath : null;
			})
			.filter((x) => x !== null) as string[];

		const { data, error } = await this.#itemRequestManager.getItems(paths);

		if (data) {
			const items: Array<UmbPartialViewItemModel> = data.map((item) => {
				return {
					entityType: item.isFolder ? UMB_PARTIAL_VIEW_FOLDER_ENTITY_TYPE : UMB_PARTIAL_VIEW_ENTITY_TYPE,
					unique: this.#serverFilePathUniqueSerializer.toUnique(item.path),
					parentUnique: item.parent ? this.#serverFilePathUniqueSerializer.toUnique(item.parent.path) : null,
					name: item.name,
					isFolder: item.isFolder,
					path: item.path,
				};
			});

			return { data: items };
		}

		return { error };
	}
}
