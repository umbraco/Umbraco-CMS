import { UmbTreeItemContextBase } from '../../../shared/components/tree/tree-item-base/tree-item-base.context';
import { urlFriendlyPathFromServerFilePath } from '../../utils';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO get unique method from an entity repository static method
export class UmbFileSystemTreeItemContext extends UmbTreeItemContextBase<FileSystemTreeItemPresentationModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: FileSystemTreeItemPresentationModel) => x.path);
	}

	constructPath(pathname: string, entityType: string, path: string) {
		return `section/${pathname}/workspace/${entityType}/edit/${urlFriendlyPathFromServerFilePath(path)}`;
	}
}
