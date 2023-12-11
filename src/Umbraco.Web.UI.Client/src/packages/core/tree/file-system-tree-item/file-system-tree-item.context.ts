import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import { UmbFileSystemTreeItemModel } from '../types.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
/**
 * Tree item context for file system tree items. Uses the path as the unique identifier.
 *
 * @export
 * @class UmbFileSystemTreeItemContext
 * @extends {UmbTreeItemContextBase<UmbFileSystemTreeItemModel>}
 */
export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<UmbFileSystemTreeItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: UmbFileSystemTreeItemModel) => x.path);
	}

	//TODO: this is not a nice solution! There should be a better way to distinguish in the tree between folders and files. Additionally we need to be able to register an action only on empty folder.

	checkIfIsFolder() {
		const treeItem = this.getTreeItem();
		if (treeItem?.isFolder) {
			if (treeItem.hasChildren) {
				this.entityType = `${this.getTreeItem()?.entityType}-folder`;
			} else {
				this.entityType = `${this.getTreeItem()?.entityType}-folder-empty`;
			}
		}
	}

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${encodeURIComponent(path).replace('.', '-')}`;
	}
}
