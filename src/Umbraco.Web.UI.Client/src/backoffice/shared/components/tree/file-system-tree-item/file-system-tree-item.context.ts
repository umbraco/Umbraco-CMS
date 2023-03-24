import { UmbTreeItemContextBase } from '../tree-item-base/tree-item-base.context';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { urlFriendlyPathFromServerPath } from 'src/backoffice/templating/utils';

// TODO get unique method from an entity repository static method
export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<FileSystemTreeItemPresentationModel> {
	constructor(host: UmbControllerHostInterface, treeItem: FileSystemTreeItemPresentationModel) {
		super(host, treeItem, (x: FileSystemTreeItemPresentationModel) => x.path);
	}

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${urlFriendlyPathFromServerPath(path)}`;
	}
}
