import type { UmbFileSystemMockDbBase } from './file-system-base.js';
import type { CreateFileRequestModel, FileResponseModel, UpdateFileRequestModel } from './types.js';

export class UmbMockFileSystemDetailManager<MockItemType extends FileResponseModel> {
	#db: UmbFileSystemMockDbBase<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>) {
		this.#db = db;
	}

	create(request: CreateFileRequestModel) {
		let path = request.parent ? `${request.parent.path}/${request.name}` : request.name;
		// ensure dash prefix if its not there
		path = path.startsWith('/') ? path : `/${path}`;

		const mockItem = this.#defaultCreateMockItemMapper(path, request);
		// create mock item in mock db
		this.#db.create(mockItem);
		return path;
	}

	read(path: string): FileResponseModel | undefined {
		const item = this.#db.read(path);
		if (!item) throw new Error('Item not found');
		const mappedItem = this.#defaultReadResponseMapper(item);
		return mappedItem;
	}

	update(path: string, item: UpdateFileRequestModel) {
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

	rename(path: string, name: string) {
		const currentMockItem = this.#db.read(path);
		if (!currentMockItem) throw new Error('Item not found');

		const createRequest: CreateFileRequestModel = {
			name,
			parent: currentMockItem.parent,
			content: currentMockItem.content,
		};

		this.delete(path);
		const newPath = this.create(createRequest);
		return newPath;
	}

	#defaultCreateMockItemMapper = (path: string, request: CreateFileRequestModel): MockItemType => {
		return {
			name: request.name,
			content: request.content,
			path: path,
			parent: request.parent || null,
			isFolder: false,
			hasChildren: false,
		} as unknown as MockItemType;
	};

	#defaultReadResponseMapper = (item: MockItemType): FileResponseModel => {
		return {
			path: item.path,
			parent: item.parent,
			name: item.name,
			content: item.content,
		};
	};
}
