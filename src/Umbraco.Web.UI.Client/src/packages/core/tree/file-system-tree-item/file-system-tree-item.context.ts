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

	checkIfIsFolder() {
		if (this.getTreeItem()?.isFolder) {
			this.type = `${this.getTreeItem()?.type}-folder`;
		}
	}

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${encodeURIComponent(path).replace('.', '-')}`;
	}
}
