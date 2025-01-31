import type { UmbFileSystemMockDbBase } from './file-system-base.js';

export class UmbMockFileSystemFolderManager<
	MockItemType extends { path: string; name: string; parent?: { path: string } | null; isFolder: boolean },
> {
	#db: UmbFileSystemMockDbBase<MockItemType>;

	constructor(db: UmbFileSystemMockDbBase<MockItemType>) {
		this.#db = db;
	}

	create(request: any) {
		let path = request.parent ? `${request.parent.path}/${request.name}` : request.name;
		// ensure dash prefix if its not there
		path = path.startsWith('/') ? path : `/${path}`;

		const newFolder = {
			path,
			parent: request.parent ? { path: request.parent.path } : null,
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

	#defaultReadMapper = (item: MockItemType) => {
		return {
			path: item.path,
			name: item.name,
			parent: item.parent,
		};
	};
}
