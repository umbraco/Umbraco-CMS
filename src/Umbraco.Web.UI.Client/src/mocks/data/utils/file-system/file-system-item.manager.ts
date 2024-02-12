import type { UmbMockDBBase } from '../mock-db-base.js';
import type { FileSystemItemResponseModelBaseModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockFileSystemItemManager<T extends FileSystemItemResponseModelBaseModel> {
	#db: UmbMockDBBase<T>;

	constructor(db: UmbMockDBBase<T>) {
		this.#db = db;
	}

	getItems(paths: Array<string>) {
		const items = this.#db.getAll().filter((item) => paths.includes(item.path));
		return items.map((item) => createFileItemResponseModelBaseModel(item));
	}
}

const createFileItemResponseModelBaseModel = (item: any): FileSystemItemResponseModelBaseModel => ({
	path: item.path,
	name: item.name,
	parent: item.parent,
	isFolder: item.isFolder,
});
