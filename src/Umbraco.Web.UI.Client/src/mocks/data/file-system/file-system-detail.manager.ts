import { UmbFileSystemMockDbBase } from './file-system-base.js';
import { getParentPathFromServerPath } from './util/index.js';
import {
	CreateTextFileViewModelBaseModel,
	ScriptResponseModel,
	UpdateTextFileViewModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbMockFileSystemDetailManagerArgs<MockItemType extends ScriptResponseModel> {
	createMapper: (item: CreateTextFileViewModelBaseModel) => MockItemType;
	readMapper: (item: MockItemType) => ScriptResponseModel;
}

export class UmbMockFileSystemDetailManager<MockItemType extends ScriptResponseModel> {
	#db: UmbFileSystemMockDbBase<MockItemType>;
	#args: UmbMockFileSystemDetailManagerArgs<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>, args: UmbMockFileSystemDetailManagerArgs<MockItemType>) {
		this.#db = db;
		this.#args = args;
	}

	create(item: CreateTextFileViewModelBaseModel) {
		const mockItem = this.#args.createMapper(item);
		// create mock item in mock db
		this.#db.create(mockItem);
	}

	read(path: string): ScriptResponseModel | undefined {
		const item = this.#db.read(path);
		// map mock item to response model
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		const mappedItem = this.#args.readMapper(item);
		return mappedItem;
	}

	update(item: UpdateTextFileViewModelBaseModel) {
		const mockItem = this.#db.read(item.existingPath);

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

	delete(path: string) {
		this.#db.delete(path);
	}
}
