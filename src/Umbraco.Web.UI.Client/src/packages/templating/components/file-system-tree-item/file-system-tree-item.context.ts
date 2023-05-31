import { urlFriendlyPathFromServerFilePath } from '../../utils.js';
import { UmbTreeItemContextBase } from '@umbraco-cms/backoffice/tree';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<FileSystemTreeItemPresentationModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: FileSystemTreeItemPresentationModel) => x.path);
	}

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${urlFriendlyPathFromServerFilePath(path)}`;
	}
}
