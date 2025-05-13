import type { UmbDocumentUrlModel } from './repository/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A controller for resolving data for document urls
 * @exports
 * @class UmbDocumentUrlsDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentUrlsDataResolver extends UmbControllerBase {
	#appCulture?: string;
	#propertyDataSetCulture?: UmbVariantId;
	#data?: Array<UmbDocumentUrlModel> | undefined;

	#init: Promise<unknown>;

	#urls = new UmbArrayState<UmbDocumentUrlModel>([], (url) => url.url);
	/**
	 * The urls for the current culture
	 * @returns {ObservableArray<UmbDocumentUrlModel>} The urls for the current culture
	 * @memberof UmbDocumentUrlsDataResolver
	 */
	public readonly urls = this.#urls.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		// TODO: listen for UMB_VARIANT_CONTEXT when available
		this.#init = Promise.all([
			this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
				this.#propertyDataSetCulture = context?.getVariantId();
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

	#setCultureAwareValues() {
		this.#setUrls();
	}

	#setUrls() {
		const data = this.#getDataForCurrentCulture();
		this.#urls.setValue(data ?? []);
	}

	#getCurrentCulture(): string | undefined {
		return this.#propertyDataSetCulture?.culture || this.#appCulture;
	}

	#getDataForCurrentCulture(): Array<UmbDocumentUrlModel> | undefined {
		const culture = this.#getCurrentCulture();
		// If there is no culture context (invariant data) we return all urls
		return culture ? this.#data?.filter((x) => x.culture === culture) : this.#data;
	}
}
