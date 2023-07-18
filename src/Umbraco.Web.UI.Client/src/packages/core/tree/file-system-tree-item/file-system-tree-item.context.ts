import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<FileSystemTreeItemPresentationModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: FileSystemTreeItemPresentationModel) => x.path);
	}
}
