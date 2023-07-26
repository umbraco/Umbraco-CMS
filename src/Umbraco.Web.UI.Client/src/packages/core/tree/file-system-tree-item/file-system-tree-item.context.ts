import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
/**
 * Tree item context for file system tree items. Uses the path as the unique identifier.
 *
 * @export
 * @class UmbFileSystemTreeItemContext
 * @extends {UmbTreeItemContextBase<FileSystemTreeItemPresentationModel>}
 */
export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<FileSystemTreeItemPresentationModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: FileSystemTreeItemPresentationModel) => x.path);
	}

	//TODO: this is not a nice solution! There should be a better way to distinguish in the tree between folders and files. Additionally we need to be able to register an action only on empty folder.

	checkIfIsFolder() {
		const treeItem = this.getTreeItem();
		if (treeItem?.isFolder) {
			if (treeItem.hasChildren) {
				this.type = `${this.getTreeItem()?.type}-folder`;
			} else {
				this.type = `${this.getTreeItem()?.type}-folder-empty`;
			}
		}
	}

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${encodeURIComponent(path).replace('.', '-')}`;
	}
}
