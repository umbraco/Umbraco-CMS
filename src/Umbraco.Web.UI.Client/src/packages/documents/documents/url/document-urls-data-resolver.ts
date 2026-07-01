import type { UmbDocumentUrlModel } from './repository/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_VARIANT_CONTEXT, type UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A controller for resolving data for document urls
 * @exports
 * @class UmbDocumentUrlsDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentUrlsDataResolver extends UmbControllerBase {
	#appCulture?: string;
	#variantId?: UmbVariantId;
	#displayVariantId?: UmbVariantId;
	#data?: Array<UmbDocumentUrlModel> | undefined;

	#init: Promise<unknown>;

	#urls = new UmbArrayState<UmbDocumentUrlModel>([], (url) => url.url);
	/**
	 * The urls for the current culture
	 * @returns {ObservableArray<UmbDocumentUrlModel>} The urls for the current culture
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	public readonly urls = this.#urls.asObservable();

	#requestCulture = new UmbStringState<string | undefined>(undefined);
	/**
	 * The culture to request urls for from the server. Emits whenever the displayed culture changes.
	 * Resolves to undefined for invariant documents (meaning all cultures).
	 * @returns {Observable<string | undefined>} The culture to request urls for
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	public readonly requestCulture = this.#requestCulture.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_VARIANT_CONTEXT, async (context) => {
				this.#variantId = await context?.getVariantId();

				// Observe the display variant id so the resolver reacts to culture switches, rather than
				// capturing the culture only once.
				if (context) {
					this.observe(
						context.displayVariantId,
						(displayVariantId) => {
							this.#displayVariantId = displayVariantId;
							this.#setCultureAwareValues();
						},
						'observeDisplayVariantId',
					);
				}

				this.#setCultureAwareValues();
			}).asPromise(),
		]);
	}

	/**
	 * Get the current data
	 * @returns {Array<UmbDocumentUrlModel> | undefined} The current data
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	getData(): Array<UmbDocumentUrlModel> | undefined {
		return this.#data;
	}

	/**
	 * Set the current data
	 * @param {Array<UmbDocumentUrlModel> | undefined} data The current data
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	setData(data: Array<UmbDocumentUrlModel> | undefined) {
		this.#data = data;

		if (!this.#data) {
			this.#urls.setValue([]);
			return;
		}

		this.#setCultureAwareValues();
	}

	/**
	 * Get the urls for the current culture
	 * @returns {(Promise<Array<UmbDocumentUrlModel> | []>)} The urls for the current culture
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	async getUrls(): Promise<Array<UmbDocumentUrlModel> | []> {
		await this.#init;
		return this.#urls.getValue();
	}

	/**
	 * Gets the culture to request urls for from the server.
	 * Returns the current culture for variant documents, or undefined for invariant documents
	 * (which must return all of their domain urls).
	 * @returns {Promise<string | undefined>} The culture to request, or undefined for all cultures
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	async getRequestCulture(): Promise<string | undefined> {
		await this.#init;
		return this.#requestCulture.getValue();
	}

	#setCultureAwareValues() {
		this.#setUrls();
		this.#requestCulture.setValue(this.#variantId?.isCultureInvariant() ? undefined : this.#getCurrentCulture());
	}

	#setUrls() {
		const data = this.#getDataForCurrentCulture();
		this.#urls.setValue(data ?? []);
	}

	#getCurrentCulture(): string | undefined {
		return this.#variantId?.culture || this.#displayVariantId?.culture || this.#appCulture;
	}

	#getDataForCurrentCulture(): Array<UmbDocumentUrlModel> | undefined {
		// Invariant document: return all URLs. The server tags each URL with the
		// culture of the domain that produced it.
		if (this.#variantId?.isCultureInvariant()) {
			return this.#data;
		}

		// Variant document: filter to the culture currently being viewed.
		// If no culture can be resolved at all, fall back to returning everything.
		const culture = this.#getCurrentCulture();
		return culture ? this.#data?.filter((x) => x.culture === culture) : this.#data;
	}
}
