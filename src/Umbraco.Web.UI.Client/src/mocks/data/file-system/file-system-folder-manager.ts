import { UmbData } from '../data.js';
import { CreatePathFolderRequestModel, PathFolderModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMockFileSystemFolderManager<T extends PathFolderModelBaseModel> {
	#db: UmbData<T>;

	constructor(db: UmbData<T>) {
		this.#db = db;
	}

	create(request: CreatePathFolderRequestModel) {
		const newFolder = {
			path: `${request.parentPath ?? ''}/${request.name}`,
			name: request.name,
			hasChildren: false,
			isFolder: true,
		};

		this.#db.getData().push(newFolder);
	}

	read(path: string) {
		return this.#db.getData().find((item) => item.path === path);
	}

	delete(path: string) {
		alert('delete folder');
	}
}
