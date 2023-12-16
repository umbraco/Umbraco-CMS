import { UmbData } from '../data.js';
import { createFileItemResponseModelBaseModel } from '../utils.js';
import { FileItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemItemManager<T extends FileItemResponseModelBaseModel> {
	#db: UmbData<T>;

	constructor(db: UmbData<T>) {
		this.#db = db;
	}

	getItems(paths: Array<string>) {
		const items = this.#db.getData().filter((item) => paths.includes(item.path));
		return items.map((item) => createFileItemResponseModelBaseModel(item));
	}
}
