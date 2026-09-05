import type { UmbMockDBBase } from '../mock-db-base.js';
import type { FileItemResponseModel } from './types.js';

export class UmbMockFileSystemItemManager<T extends { path: string }> {
	#db: UmbMockDBBase<T>;

	constructor(db: UmbMockDBBase<T>) {
		this.#db = db;
	}

	getItems(paths: Array<string>) {
		const items = this.#db.getAll().filter((item) => paths.includes(item.path));
		return items.map((item) => createFileItemResponseModelBaseModel(item));
	}
}

const createFileItemResponseModelBaseModel = (item: any): FileItemResponseModel => ({
	path: item.path,
	name: item.name,
	parent: item.parent,
	isFolder: item.isFolder,
});
