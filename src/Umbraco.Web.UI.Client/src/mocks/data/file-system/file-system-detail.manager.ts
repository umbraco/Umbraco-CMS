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

	create(request: FileSystemFileCreateRequestModelBaseModel) {
		let path = request.parent ? `${request.parent.path}/${request.name}` : request.name;
		// ensure dash prefix if its not there
		path = path.startsWith('/') ? path : `/${path}`;

		const mockItem = this.#defaultCreateMockItemMapper(path, request);
		// create mock item in mock db
		this.#db.create(mockItem);
		return path;
	}

	read(path: string): FileSystemResponseModelBaseModel | undefined {
		const item = this.#db.read(path);
		if (!item) throw new Error('Item not found');
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

	#defaultCreateMockItemMapper = (path: string, request: FileSystemFileCreateRequestModelBaseModel): MockItemType => {
		return {
			name: request.name,
			content: request.content,
			path: path,
			parent: request.parent || null,
			isFolder: false,
			hasChildren: false,
		} as unknown as MockItemType;
	};

	#defaultReadResponseMapper = (item: MockItemType): FileSystemFileResponseModelBaseModel => {
		return {
			path: item.path,
			parent: item.parent,
			name: item.name,
			content: item.content,
		};
	};
}
