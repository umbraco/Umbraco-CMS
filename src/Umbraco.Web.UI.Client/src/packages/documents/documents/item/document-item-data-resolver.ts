import { UmbDocumentVariantState } from '../types.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbAppLanguageContext } from '@umbraco-cms/backoffice/language';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
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

	#init: Promise<[UmbAppLanguageContext]>;

	#unique = new UmbStringState(undefined);
	public readonly unique = this.#unique.asObservable();

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#icon = new UmbStringState(undefined);
	public readonly icon = this.#icon.asObservable();

	#isTrashed = new UmbBooleanState(undefined);
	public readonly isTrashed = this.#isTrashed.asObservable();

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		// We do not depend on this context because we know is it only available in some cases
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#propertyDataSetCulture = context.getVariantId();
			this.#setName();
			this.#setIsDraft();
		});

		this.#init = Promise.all([
			this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
				this.observe(context.appLanguageCulture, (culture) => {
					this.#appCulture = culture;
					this.#setName();
					this.#setIsDraft();
				});

				this.observe(context.appDefaultLanguage, (value) => {
					this.#defaultCulture = value?.unique;
					this.#setName();
					this.#setIsDraft();
				});
			}).asPromise(),
		]);
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

		if (!this.#item) {
			this.#unique.setValue(undefined);
			this.#name.setValue(undefined);
			this.#icon.setValue(undefined);
			this.#isTrashed.setValue(undefined);
			this.#isDraft.setValue(undefined);
			return;
		}

		this.#unique.setValue(this.#item.unique);
		this.#icon.setValue(this.#item.documentType.icon);
		this.#isTrashed.setValue(this.#item.isTrashed);
	}

	/**
	 * Get the unique of the item
	 * @returns {Promise<string | undefined>} The unique of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		await this.#init;
		return this.#unique.getValue();
	}

	/**
	 * Get the name of the item
	 * @returns {Promise<string>} The name of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getName(): Promise<string> {
		await this.#init;
		return this.#name.getValue() || '';
	}

	/**
	 * Get the icon of the item
	 * @returns {Promise<string | undefined>} The icon of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIcon(): Promise<string | undefined> {
		await this.#init;
		return this.#item?.documentType.icon;
	}

	/**
	 * Get the state of the item
	 * @returns {Promise<string | undefined>} The state of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getState(): Promise<DocumentVariantStateModel | null | undefined> {
		await this.#init;
		return this.#getCurrentVariant()?.state;
	}

	/**
	 * Get the isDraft of the item
	 * @returns {Promise<boolean>} The isDraft of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIsDraft(): Promise<boolean> {
		await this.#init;
		return this.#isDraft.getValue() ?? false;
	}

	/**
	 * Get the isTrashed of the item
	 * @returns {Promise<boolean | undefined>} The isTrashed of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIsTrashed(): Promise<boolean> {
		await this.#init;
		return this.#item?.isTrashed ?? false;
	}

	#setName() {
		const variant = this.#getCurrentVariant();
		const fallbackName = this.#findVariant(this.#defaultCulture)?.name;
		const name = variant?.name ?? `(${fallbackName})`;
		this.#name.setValue(name);
	}

	#setIsDraft() {
		const variant = this.#getCurrentVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
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
