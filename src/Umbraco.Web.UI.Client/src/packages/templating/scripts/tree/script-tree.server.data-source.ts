import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../entity.js';
import { UmbServerPathUniqueSerializer } from '../../utils/index.js';
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
	const serializer = new UmbServerPathUniqueSerializer();

	if (parentUnique === null) {
		return getRootItems();
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return ScriptResource.getTreeScriptChildren({
			parentPath: serializer.toServerPath(parentUnique),
		});
	}
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbScriptTreeItemModel => {
	const serializer = new UmbServerPathUniqueSerializer();

	return {
		unique: serializer.toUnique(item.path),
		parentUnique: item.parent ? serializer.toUnique(item.parent.path) : null,
		entityType: item.isFolder ? UMB_SCRIPT_FOLDER_ENTITY_TYPE : UMB_SCRIPT_ENTITY_TYPE,
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		isContainer: false,
	};
};
