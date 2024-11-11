import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { UmbScriptItemModel } from '../../types.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ScriptService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source for script items that fetches data from the server
 * @class UmbScriptItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbScriptItemServerDataSource implements UmbItemDataSource<UmbScriptItemModel> {
	#host: UmbControllerHost;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	/**
	 * Creates an instance of UmbScriptItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbScriptItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given uniques from the server
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbScriptItemServerDataSource
	 */
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const paths = uniques
			.map((unique) => {
				const serverPath = this.#serverFilePathUniqueSerializer.toServerPath(unique);
				return serverPath ? serverPath : null;
			})
			.filter((x) => x !== null) as string[];

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			ScriptService.getItemScript({
				path: paths,
			}),
		);

		if (data) {
			const items: Array<UmbScriptItemModel> = data.map((item) => {
				return {
					entityType: item.isFolder ? UMB_SCRIPT_FOLDER_ENTITY_TYPE : UMB_SCRIPT_ENTITY_TYPE,
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
