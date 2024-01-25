import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import type { UmbFileSystemTreeItemModel } from '../types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
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

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${encodeURIComponent(path).replace('.', '-')}`;
	}
}
