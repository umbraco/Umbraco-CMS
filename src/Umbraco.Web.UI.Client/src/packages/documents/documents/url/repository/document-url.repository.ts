import type { UmbDocumentUrlsModel } from './types.js';
import { UMB_DOCUMENT_URL_STORE_CONTEXT } from './document-url.store.context-token.js';
import { UmbDocumentUrlServerDataSource } from './document-url.server.data-source.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentUrlRepository extends UmbItemRepositoryBase<UmbDocumentUrlsModel> {
	// The culture-aware request needs its own data source instance: `UmbItemRepositoryBase` keeps its
	// data source private and the inherited `requestItems` signature cannot carry a culture. Extending
	// the item base keeps this consistent with `UmbMediaUrlRepository`; `requestUrls` otherwise mirrors
	// the base flow, including store population.
	readonly #urlSource = new UmbDocumentUrlServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentUrlServerDataSource, UMB_DOCUMENT_URL_STORE_CONTEXT);
	}

	/**
	 * Requests the urls for the given uniques, optionally restricted to a single culture.
	 * @param {Array<string>} uniques The document uniques to request urls for.
	 * @param {string} [culture] The culture to restrict variant document urls to, or undefined for all cultures.
	 * @returns {*} The requested url data.
	 * @memberof UmbDocumentUrlRepository
	 */
	async requestUrls(uniques: Array<string>, culture?: string) {
		if (!uniques) throw new Error('Uniques are missing');

		await this._init;

		const { data, error } = await this.#urlSource.getItems(uniques, culture);

		if (data) {
			this._itemStore?.appendItems(data);
		}

		return { data, error, asObservable: () => this._itemStore?.items(uniques) };
	}
}

export { UmbDocumentUrlRepository as api };
