import { UmbMediaUrlServerDataSource } from './media-url.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaUrlRepository extends UmbRepositoryBase {
	#urlDataSource: UmbMediaUrlServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#urlDataSource = new UmbMediaUrlServerDataSource(this);
	}

	/**
	 * Read the urls of the media items
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @return {*}
	 * @memberof UmbMediaUrlRepository
	 */
	async readUrls(uniques: Array<string>) {
		if (!uniques) throw new Error('uniques are missing');
		const { data } = await this.#urlDataSource.getUrls(uniques);
		return { data };
	}
}
