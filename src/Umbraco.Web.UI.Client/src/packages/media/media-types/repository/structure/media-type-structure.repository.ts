import { UmbMediaTypeStructureServerDataSource } from './media-type-structure.server.data-source.js';
import type { UmbAllowedMediaTypeModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContentTypeStructureRepositoryBase } from '@umbraco-cms/backoffice/content-type';
import { fetchAllPages, type UmbOffsetPageFetcher } from '@umbraco-cms/backoffice/repository';

// Mirrors the server's default `take` for the media type item endpoints.
const MEDIA_TYPE_PAGE_SIZE = 100;

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

	/**
	 * Returns the media types that allow the given file extension. Pages through them all unless an explicit
	 * `skip`/`take` is given, in which case only that page is returned.
	 * @param {object} args - The file extension to match, and optionally the single page to return.
	 * @returns {Promise} A promise resolving to the matching media types, or an empty array if the request failed.
	 * @memberof UmbMediaTypeStructureRepository
	 */
	async requestMediaTypesOf({ fileExtension, skip, take }: { fileExtension: string; skip?: number; take?: number }) {
		const { data } = await this.#requestPagedOrAll(
			(pageSkip, pageTake) =>
				this.#mediaTypeDataSource.getMediaTypesOfFileExtension({ fileExtension, skip: pageSkip, take: pageTake }),
			{ skip, take },
		);

		return data?.items ?? [];
	}

	/**
	 * Returns the media types that represent folders. Pages through them all unless an explicit `skip`/`take` is
	 * given, in which case only that page is returned.
	 * @param {object} args - Optionally the single page to return.
	 * @returns {Promise} A promise resolving to the folder media types, or an empty array if the request failed.
	 * @memberof UmbMediaTypeStructureRepository
	 */
	async requestMediaTypesOfFolders({ skip, take }: { skip?: number; take?: number } = {}) {
		const { data } = await this.#requestPagedOrAll(
			(pageSkip, pageTake) => this.#mediaTypeDataSource.getMediaTypesOfFolders({ skip: pageSkip, take: pageTake }),
			{ skip, take },
		);

		return data?.items ?? [];
	}

	#requestPagedOrAll(
		fetchPage: UmbOffsetPageFetcher<UmbAllowedMediaTypeModel>,
		{ skip, take }: { skip?: number; take?: number },
	) {
		if (skip !== undefined || take !== undefined) {
			return fetchPage(skip ?? 0, take ?? MEDIA_TYPE_PAGE_SIZE);
		}

		return fetchAllPages<UmbAllowedMediaTypeModel>(fetchPage, MEDIA_TYPE_PAGE_SIZE);
	}
}

export default UmbMediaTypeStructureRepository;
