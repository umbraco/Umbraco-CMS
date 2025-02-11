import type { UmbDocumentItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A controller for resolving data for a document item
 * @exports
 * @class UmbDocumentItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentItemDataResolver extends UmbControllerBase {
	#defaultCulture?: string;
	#appCulture?: string;
	#propertyDataSetCulture?: UmbVariantId;
	#item?: UmbDocumentItemModel | undefined;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(context.appLanguageCulture, (culture) => (this.#appCulture = culture));
			this.observe(context.appDefaultLanguage, (value) => {
				this.#defaultCulture = value?.unique;
			});
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#propertyDataSetCulture = context.getVariantId();
		});
	}

	/**
	 * Get the current item
	 * @returns {UmbDocumentItemModel | undefined} The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getItem(): UmbDocumentItemModel | undefined {
		return this.#item;
	}

	/**
	 * Set the current item
	 * @param {UmbDocumentItemModel | undefined} item The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	setItem(item: UmbDocumentItemModel | undefined) {
		this.#item = item;
	}

	/**
	 * Get the unique of the item
	 * @returns {string | undefined} The unique of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getUnique(): string | undefined {
		return this.#item?.unique;
	}

	/**
	 * Get the name of the item
	 * @returns {string} The name of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getName(): string {
		const variant = this.#getCurrentVariant();
		const fallbackName = this.#findVariant(this.#defaultCulture)?.name;

		return variant?.name ?? `(${fallbackName})`;
	}

	/**
	 * Get the icon of the item
	 * @returns {string | undefined} The icon of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getIcon(): string | undefined {
		return this.#item?.documentType.icon;
	}

	/**
	 * Get the state of the item
	 * @returns {string | undefined} The state of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getState(): DocumentVariantStateModel | null | undefined {
		return this.#getCurrentVariant()?.state;
	}

	/**
	 * Get the isTrashed of the item
	 * @returns {boolean | undefined} The isTrashed of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	isTrashed(): boolean {
		return this.#item?.isTrashed ?? false;
	}

	#findVariant(culture: string | undefined) {
		return this.#item?.variants.find((x) => x.culture === culture);
	}

	#getCurrentVariant() {
		if (this.#isInvariant()) {
			return this.#item?.variants?.[0];
		}

		const culture = this.#propertyDataSetCulture?.culture || this.#appCulture;
		return this.#findVariant(culture);
	}

	#isInvariant() {
		return this.#item?.variants?.[0]?.culture === null;
	}
}
