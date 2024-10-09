import { UmbMediaTypeStructureServerDataSource } from './media-type-structure.server.data-source.js';
import type { UmbAllowedMediaTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

export class UmbMediaTypeStructureRepository extends UmbContentTypeStructureRepositoryBase<UmbAllowedMediaTypeModel> {
	#dataSource;
	constructor(host: UmbControllerHost) {
		super(host, UmbMediaTypeStructureServerDataSource);
		this.#dataSource = new UmbMediaTypeStructureServerDataSource(host);
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
		return this.#dataSource.getMediaTypesOfFileExtension({ fileExtension, skip, take });
	}

	async requestMediaTypesOfFolders({ skip = 0, take = 100 } = {}) {
		return this.#dataSource.getMediaTypesOfFolders({ skip, take });
	}
}

export default UmbMediaTypeStructureRepository;
