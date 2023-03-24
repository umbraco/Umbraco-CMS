import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO get unique method from an entity repository static method
export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<FileSystemTreeItemPresentationModel> {
	constructor(host: UmbControllerHostInterface, treeItem: FileSystemTreeItemPresentationModel) {
		super(host, treeItem, (x: FileSystemTreeItemPresentationModel) => x.path);
	}
}
