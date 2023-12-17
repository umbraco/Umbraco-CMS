import { UmbFileSystemMockDbBase } from './file-system-base.js';
import { CreatePathFolderRequestModel, PathFolderModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemFolderManager<MockItemType extends { path: string }> {
	#db: UmbFileSystemMockDbBase<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>) {
		this.#db = db;
	}

	create(request: CreatePathFolderRequestModel) {
		const newFolder = {
			path: request.parentPath ? `${request.parentPath}/${request.name}` : request.name,
			parenPath: request.parentPath,
			name: request.name,
			hasChildren: false,
			isFolder: true,
		};

		this.#db.create(newFolder);
	}

	read(path: string) {
		const dbItem = this.#db.read(path);
		const isFolder = dbItem?.isFolder ?? false;
		if (!isFolder) return undefined;
		return fileSystemFolderMapper<MockItemType>(dbItem);
	}

	delete(path: string) {
		const dbItem = this.#db.read(path);
		const isFolder = dbItem?.isFolder ?? false;
		if (isFolder) {
			this.#db.delete(path);
		}
	}
}

const fileSystemFolderMapper = (item: T): PathFolderModelBaseModel => {
	return {
		name: item.name,
		path: item.path,
		hasChildren: item.hasChildren ?? false,
		isFolder: item.isFolder ?? false,
	};
};
