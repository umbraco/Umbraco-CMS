import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbScriptTreeItemModel } from './types.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ScriptResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Script tree that fetches data from the server
 * @export
 * @class UmbScriptTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbScriptTreeServerDataSource extends UmbTreeServerDataSourceBase<
	FileSystemTreeItemPresentationModel,
	UmbScriptTreeItemModel
> {
	/**
	 * Creates an instance of UmbScriptTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbScriptTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			mapper,
		});
	}
}

// eslint-disable-next-line local-rules/no-direct-api-import
const getRootItems = () => ScriptResource.getTreeScriptRoot({});

const getChildrenOf = (parentUnique: string | null) => {
	const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(parentUnique);

	if (parentPath === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return ScriptResource.getTreeScriptChildren({
			parentPath,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbScriptTreeItemModel => {
	const serializer = new UmbServerFilePathUniqueSerializer();

	return {
		unique: serializer.toUnique(item.path),
		parentUnique: item.parent ? serializer.toUnique(item.parent.path) : null,
		entityType: item.isFolder ? UMB_SCRIPT_FOLDER_ENTITY_TYPE : UMB_SCRIPT_ENTITY_TYPE,
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		icon: item.isFolder ? undefined : 'icon-diploma',
	};
};
