import { UmbMediaTypeStructureServerDataSource } from './media-type-structure.server.data-source.js';
import type { UmbAllowedMediaTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';

/**
 * Shared across every repository instance: which media types are folders is a property of the installation, and
 * several unrelated components need it to render a single item. Resolved once so they do not each pay a request.
 */
let folderTypeUniquesPromise: Promise<ReadonlySet<string>> | undefined;

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

	/**
	 * The media types that represent a folder, i.e. one that holds other media rather than a file.
	 * @returns {Promise<ReadonlySet<string>>} The unique of every folder media type.
	 * @description Memoised for the lifetime of the page, unlike {@link requestMediaTypesOfFolders}, which always
	 * asks the server. Use this where the answer only decides how an item is presented.
	 */
	async getFolderTypeUniques(): Promise<ReadonlySet<string>> {
		folderTypeUniquesPromise ??= this.requestMediaTypesOfFolders()
			.then(
				(folderTypes) =>
					new Set(
						folderTypes
							.map((folderType) => folderType.unique)
							.filter((unique): unique is string => typeof unique === 'string'),
					),
			)
			.catch((error) => {
				// Memoising a failure would keep every later caller broken until the page is reloaded.
				folderTypeUniquesPromise = undefined;
				throw error;
			});
		return await folderTypeUniquesPromise;
	}
}

export default UmbMediaTypeStructureRepository;
