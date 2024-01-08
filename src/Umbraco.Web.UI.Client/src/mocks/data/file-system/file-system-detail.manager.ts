import { UmbFileSystemMockDbBase } from './file-system-base.js';
import {
	FileSystemFileCreateRequestModelBaseModel,
	FileSystemFileResponseModelBaseModel,
	FileSystemFileUpdateRequestModelBaseModel,
	FileSystemResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemDetailManager<MockItemType extends FileSystemFileResponseModelBaseModel> {
	#db: UmbFileSystemMockDbBase<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>) {
		this.#db = db;
	}

	create(item: FileSystemFileCreateRequestModelBaseModel) {
		const path = item.parentPath ? item.parentPath + '/' + item.name : item.name;
		const mockItem = this.#defaultCreateMockItemMapper(path, item);
		// create mock item in mock db
		this.#db.create(mockItem);
		return path;
	}

	read(path: string): FileSystemResponseModelBaseModel | undefined {
		const item = this.#db.read(path);
		// map mock item to response model
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		const mappedItem = this.#defaultReadResponseMapper(item);
		return mappedItem;
	}

	update(path: string, item: FileSystemFileUpdateRequestModelBaseModel) {
		const mockItem = this.#db.read(path);

		const updatedMockItem = {
			...mockItem,
			content: item.content,
		} as unknown as MockItemType;

		this.#db.update(path, updatedMockItem);
	}

	delete(path: string) {
		this.#db.delete(path);
	}

	/* TODO: implement as rename
	update(path, item: UpdateTextFileViewModelBaseModel) {
		const mockItem = this.#db.read(path);

		const parentPath = getParentPathFromServerPath(item.existingPath);
		const newPath = parentPath ? parentPath + '/' + item.name : item.name;

		const updatedMockItem = {
			...mockItem,
			name: item.name,
			content: item.content,
			path: newPath,
		} as MockItemType;

		this.#db.update(item.existingPath, updatedMockItem);
	}
	*/

	#defaultCreateMockItemMapper = (path: string, item: FileSystemFileCreateRequestModelBaseModel): MockItemType => {
		return {
			name: item.name,
			content: item.content,
			path: path,
			parentPath: item.parentPath || null,
			isFolder: false,
			hasChildren: false,
		} as unknown as MockItemType;
	};

	#defaultReadResponseMapper = (item: MockItemType): FileSystemFileResponseModelBaseModel => {
		return {
			path: item.path,
			name: item.name,
			content: item.content,
		};
	};
}
