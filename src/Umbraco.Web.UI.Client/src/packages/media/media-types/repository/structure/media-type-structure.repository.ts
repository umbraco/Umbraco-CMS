import { UmbMediaTypeStructureServerDataSource } from './media-type-structure.server.data-source.js';
import type { UmbAllowedMediaTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbMediaTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedMediaTypeModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeStructureServerDataSource);
	}

	get #mediaTypeDataSource() {
		return this._dataSource as UmbMediaTypeStructureServerDataSource;
	}

	async requestAllowedParentsOf(unique: string) {
		return this.#mediaTypeDataSource.getAllowedParentsOf(unique);
	}

	async requestMediaTypesOf({
		fileExtension,
		skip = 0,
		take = 100,
	}: {
		fileExtension: string;
		skip?: number;
		take?: number;
	}) {
		return this.#mediaTypeDataSource.getMediaTypesOfFileExtension({ fileExtension, skip, take });
	}

	async requestMediaTypesOfFolders({ skip = 0, take = 100 } = {}) {
		return this.#mediaTypeDataSource.getMediaTypesOfFolders({ skip, take });
	}
}

export default UmbMediaTypeStructureRepository;
