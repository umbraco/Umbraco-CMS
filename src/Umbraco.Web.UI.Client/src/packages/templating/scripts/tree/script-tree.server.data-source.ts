import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../entity.js';
import { UmbScriptTreeItemModel } from './types.js';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import { FileSystemTreeItemPresentationModel, ScriptResource } from '@umbraco-cms/backoffice/backend-api';
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
	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return ScriptResource.getTreeScriptChildren({
			path: parentUnique,
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbScriptTreeItemModel => {
	return {
		unique: item.path,
		parentUnique: getParentPathFromServerPath(item.path),
		entityType: item.isFolder ? UMB_SCRIPT_FOLDER_ENTITY_TYPE : UMB_SCRIPT_ENTITY_TYPE,
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		isContainer: false,
	};
};

const getParentPathFromServerPath = (serverPath: string): string | null => {
	const parentPath = serverPath.substring(0, serverPath.lastIndexOf('/'));
	return parentPath || null;
};
