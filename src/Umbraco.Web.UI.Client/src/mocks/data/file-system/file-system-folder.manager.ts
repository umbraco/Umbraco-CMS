import { UmbFileSystemMockDbBase } from './file-system-base.js';
import { CreatePathFolderRequestModel, PathFolderModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemFolderManager<
	MockItemType extends PathFolderModelBaseModel & { path: string; isFolder: boolean },
> {
	#db: UmbFileSystemMockDbBase<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>) {
		this.#db = db;
	}

	create(request: CreatePathFolderRequestModel) {
		const newFolder: MockItemType = {
			path: request.parentPath ? `${request.parentPath}/${request.name}` : request.name,
			parenPath: request.parentPath || null,
			name: request.name,
			hasChildren: false,
			isFolder: true,
			type: 'script-folder',
			icon: 'icon-script',
			content: '',
		};

		this.#db.create(newFolder);
	}

	read(path: string) {
		const mockItem = this.#db.read(path);
		if (mockItem?.isFolder) {
			return this.#defaultReadMapper(mockItem);
		} else {
			return undefined;
		}
	}

	delete(path: string) {
		const dbItem = this.#db.read(path);
		if (dbItem?.isFolder) {
			this.#db.delete(path);
		}
	}

	#defaultReadMapper = (item: MockItemType): PathFolderModelBaseModel & { path: string } => {
		return {
			name: item.name,
			path: item.path,
		};
	};
}
