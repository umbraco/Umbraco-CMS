import type { UmbEntityMockDbBase } from './entity-base.js';
import type {
	CreateFolderRequestModel,
	FolderResponseModel,
	UpdateFolderResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export class UmbMockEntityFolderManager<MockItemType extends { id: string; name: string; isFolder: boolean }> {
	#db: UmbEntityMockDbBase<MockItemType>;
	#createMockFolderMapper: (request: CreateFolderRequestModel) => MockItemType;

	constructor(
		db: UmbEntityMockDbBase<MockItemType>,
		createMockFolderMapper: (request: CreateFolderRequestModel) => MockItemType,
	) {
		this.#db = db;
		this.#createMockFolderMapper = createMockFolderMapper;
	}

	create(request: CreateFolderRequestModel) {
		const newFolder = this.#createMockFolderMapper(request);
		this.#db.create(newFolder);
		return newFolder.id;
	}

	read(id: string) {
		const mockItem = this.#db.read(id);
		if (mockItem?.isFolder) {
			return this.#defaultReadMapper(mockItem);
		} else {
			return undefined;
		}
	}

	update(id: string, request: UpdateFolderResponseModel) {
		const mockItem = this.#db.read(id);

		const updatedMockItem = {
			...mockItem,
			name: request.name,
		} as unknown as MockItemType;

		this.#db.update(id, updatedMockItem);
	}

	delete(id: string) {
		const dbItem = this.#db.read(id);
		if (dbItem?.isFolder) {
			this.#db.delete(id);
		}
	}

	#defaultReadMapper = (item: MockItemType): FolderResponseModel => {
		return {
			name: item.name,
			id: item.id,
		};
	};
}
