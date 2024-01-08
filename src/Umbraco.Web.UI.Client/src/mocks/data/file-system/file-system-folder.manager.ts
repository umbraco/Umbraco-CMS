import { UmbFileSystemMockDbBase } from './file-system-base.js';
import {
	FileSystemCreateRequestModelBaseModel,
	FileSystemResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemFolderManager<
	MockItemType extends FileSystemResponseModelBaseModel & { isFolder: boolean },
> {
	#db: UmbFileSystemMockDbBase<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>) {
		this.#db = db;
	}

	create(request: FileSystemCreateRequestModelBaseModel) {
		const path = request.parentPath ? `${request.parentPath}/${request.name}` : request.name;

		const newFolder = {
			path,
			parent: request.parentPath ? { path: request.parentPath } : null,
			name: request.name,
			hasChildren: false,
			isFolder: true,
			content: '',
		} as unknown as MockItemType;

		this.#db.create(newFolder);

		return path;
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

	#defaultReadMapper = (item: MockItemType): FileSystemResponseModelBaseModel => {
		return {
			path: item.path,
			name: item.name,
			parent: item.parent,
		};
	};
}
