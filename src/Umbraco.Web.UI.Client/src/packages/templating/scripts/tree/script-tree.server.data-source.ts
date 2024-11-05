import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../entity.js';
import type { UmbScriptTreeItemModel } from './types.js';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';
import type { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ScriptService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Script tree that fetches data from the server
 * @class UmbScriptTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbScriptTreeServerDataSource extends UmbTreeServerDataSourceBase<
	FileSystemTreeItemPresentationModel,
	UmbScriptTreeItemModel
> {
	/**
	 * Creates an instance of UmbScriptTreeServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbScriptTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getRootItems,
			getChildrenOf,
			getAncestorsOf,
			mapper,
		});
	}
}

const getRootItems = (args: UmbTreeRootItemsRequestArgs) =>
	// eslint-disable-next-line local-rules/no-direct-api-import
	ScriptService.getTreeScriptRoot({ skip: args.skip, take: args.take });

const getChildrenOf = (args: UmbTreeChildrenOfRequestArgs) => {
	const parentPath = new UmbServerFilePathUniqueSerializer().toServerPath(args.parent.unique);

	if (parentPath === null) {
		return getRootItems(args);
	} else {
		// eslint-disable-next-line local-rules/no-direct-api-import
		return ScriptService.getTreeScriptChildren({
			parentPath,
			skip: args.skip,
			take: args.take,
		});
	}
};

const getAncestorsOf = (args: UmbTreeAncestorsOfRequestArgs) => {
	const descendantPath = new UmbServerFilePathUniqueSerializer().toServerPath(args.treeItem.unique);
	if (!descendantPath) throw new Error('Descendant path is not available');

	// eslint-disable-next-line local-rules/no-direct-api-import
	return ScriptService.getTreeScriptAncestors({
		descendantPath,
	});
};

const mapper = (item: FileSystemTreeItemPresentationModel): UmbScriptTreeItemModel => {
	const serializer = new UmbServerFilePathUniqueSerializer();

	return {
		unique: serializer.toUnique(item.path),
		parent: {
			unique: item.parent ? serializer.toUnique(item.parent.path) : null,
			entityType: item.parent ? UMB_SCRIPT_ENTITY_TYPE : UMB_SCRIPT_ROOT_ENTITY_TYPE,
		},
		entityType: item.isFolder ? UMB_SCRIPT_FOLDER_ENTITY_TYPE : UMB_SCRIPT_ENTITY_TYPE,
		name: item.name,
		isFolder: item.isFolder,
		hasChildren: item.hasChildren,
		icon: item.isFolder ? undefined : 'icon-document-js',
	};
};
