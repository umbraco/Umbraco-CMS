import type { UmbData } from '../data.js';
import type { FileSystemItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemItemManager<T extends FileSystemItemResponseModelBaseModel> {
	#db: UmbData<T>;

	constructor(db: UmbData<T>) {
		this.#db = db;
	}

	getItems(paths: Array<string>) {
		const items = this.#db.getData().filter((item) => paths.includes(item.path));
		return items.map((item) => createFileItemResponseModelBaseModel(item));
	}
}

const createFileItemResponseModelBaseModel = (item: any): FileSystemItemResponseModelBaseModel => ({
	path: item.path,
	name: item.name,
	parent: item.parent,
	isFolder: item.isFolder,
});
