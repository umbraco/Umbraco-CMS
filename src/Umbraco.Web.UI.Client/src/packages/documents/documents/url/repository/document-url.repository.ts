import type { UmbDocumentUrlsModel } from './types.js';
import { UMB_DOCUMENT_URL_STORE_CONTEXT } from './document-url.store.context-token.js';
import { UmbDocumentUrlServerDataSource } from './document-url.server.data-source.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentUrlRepository extends UmbItemRepositoryBase<UmbDocumentUrlsModel> {
	// A dedicated data source is used for the culture-aware request, as the inherited `requestItems`
	// does not carry a culture. It otherwise mirrors the base flow (store population included).
	#urlSource = new UmbDocumentUrlServerDataSource(this);

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
