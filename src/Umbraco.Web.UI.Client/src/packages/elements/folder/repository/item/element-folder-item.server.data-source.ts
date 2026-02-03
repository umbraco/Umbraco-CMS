import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UmbManagementApiElementFolderItemDataRequestManager } from './element-folder-item.server.request-manager.js';
import type { UmbElementFolderItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for Element Folder items that fetches data from the server
 * @class UmbElementFolderItemServerDataSource
 */
export class UmbElementFolderItemServerDataSource extends UmbItemServerDataSourceBase<
	FolderItemResponseModel,
	UmbElementFolderItemModel
> {
	#itemRequestManager = new UmbManagementApiElementFolderItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbElementFolderItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementFolderItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const { data, error } = await this.#itemRequestManager.getItems(uniques);

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: FolderItemResponseModel): UmbElementFolderItemModel => {
	return {
		entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
		name: item.name,
		unique: item.id,
		icon: 'icon-folder',
		flags: item.flags.map((flag) => ({ alias: flag.alias })),
	};
};
