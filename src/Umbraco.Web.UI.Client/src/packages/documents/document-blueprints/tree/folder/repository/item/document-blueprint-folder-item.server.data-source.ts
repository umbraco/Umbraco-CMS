import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../../../entity.js';
import { UmbManagementApiDocumentBlueprintFolderItemDataRequestManager } from './document-blueprint-folder-item.server.request-manager.js';
import type { UmbDocumentBlueprintFolderItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { FolderItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for Document Blueprint Folder items that fetches data from the server
 * @class UmbDocumentBlueprintFolderItemServerDataSource
 */
export class UmbDocumentBlueprintFolderItemServerDataSource extends UmbItemServerDataSourceBase<
	FolderItemResponseModel,
	UmbDocumentBlueprintFolderItemModel
> {
	#itemRequestManager = new UmbManagementApiDocumentBlueprintFolderItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbDocumentBlueprintFolderItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentBlueprintFolderItemServerDataSource
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

const mapper = (item: FolderItemResponseModel): UmbDocumentBlueprintFolderItemModel => {
	return {
		entityType: UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
		name: item.name,
		unique: item.id,
		icon: 'icon-folder',
		flags: item.flags.map((flag) => ({ alias: flag.alias })),
	};
};
