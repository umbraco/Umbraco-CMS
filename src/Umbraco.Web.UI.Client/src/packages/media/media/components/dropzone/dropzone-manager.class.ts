import type { UmbContentTypeUploadableStructureRepositoryBase } from './repository/content-type-uploadable-structure-repository-base.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbMediaTypeItemModel } from '@umbraco-cms/backoffice/media-type';
import { UmbTemporaryFileManager } from '@umbraco-cms/backoffice/temporary-file';

export class UmbDropzoneManager<T extends UmbMediaTypeItemModel = UmbMediaTypeItemModel> extends UmbControllerBase {
	#init!: Promise<unknown>;

	#fileManager = new UmbTemporaryFileManager(this);
	#repository: UmbContentTypeUploadableStructureRepositoryBase<T>;

	#parentUnique: string | null = null;

	constructor(
		host: UmbControllerHost,
		typeRepository: UmbContentTypeUploadableStructureRepositoryBase<T>,
		parentUnique: string | null,
	) {
		super(host);
		this.#repository = typeRepository;
		this.#parentUnique = parentUnique;
	}

	public async dropOneFile(file: File) {
		const matchingMediaTypes = await this.#repository.requestAllowedMediaTypesOf(file.type);
		//const options = this.#allowedMediaTypes.filter((allowedMediaType) => matchingMediaTypes.includes(allowedMediaType));
	}

	public async dropFiles(files: Array<File>) {}

	async #requestAllowedMediaTypesOf(fileExtension: string) {
		const { data } = await this.#repository.requestAllowedMediaTypesOf(fileExtension);
		//const mediaTypes = data?.filter((option) => this.#allowedMediaTypes.includes(option));
		//return { fileExtension, mediaTypes };
	}

	private _reset() {
		//
	}

	public destroy() {
		super.destroy();
	}
}
