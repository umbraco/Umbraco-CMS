import { UmbDocumentVariantState } from '../types.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

type UmbDocumentItemDataResolverModel = Omit<UmbDocumentItemModel, 'parent' | 'hasChildren'>;

/**
 * A controller for resolving data for a document item
 * @exports
 * @class UmbDocumentItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentItemDataResolver<DataType extends UmbDocumentItemDataResolverModel> extends UmbControllerBase {
	#defaultCulture?: string;
	#appCulture?: string;
	#propertyDataSetCulture?: UmbVariantId;
	#data?: DataType | undefined;

	#init: Promise<unknown>;

	#unique = new UmbStringState(undefined);
	public readonly unique = this.#unique.asObservable();

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#icon = new UmbStringState(undefined);
	public readonly icon = this.#icon.asObservable();

	#state = new UmbStringState(undefined);
	public readonly state = this.#state.asObservable();

	#isTrashed = new UmbBooleanState(undefined);
	public readonly isTrashed = this.#isTrashed.asObservable();

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#propertyDataSetCulture = context?.getVariantId();
			this.#setVariantAwareValues();
		});

		// We do not depend on this context because we know is it only available in some cases
		this.#init = this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(context?.appLanguageCulture, (culture) => {
				this.#appCulture = culture;
				this.#setVariantAwareValues();
			});

			this.observe(context?.appDefaultLanguage, (value) => {
				this.#defaultCulture = value?.unique;
				this.#setVariantAwareValues();
			});
		}).asPromise();
	}

	/**
	 * Get the current item
	 * @returns {DataType | undefined} The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getData(): DataType | undefined {
		return this.#data;
	}

	/**
	 * Set the current item
	 * @param {DataType | undefined} data The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	setData(data: DataType | undefined) {
		this.#data = data;

		if (!this.#data) {
			this.#unique.setValue(undefined);
			this.#name.setValue(undefined);
			this.#icon.setValue(undefined);
			this.#isTrashed.setValue(undefined);
			this.#isDraft.setValue(undefined);
			return;
		}

		this.#unique.setValue(this.#data.unique);
		this.#icon.setValue(this.#data.documentType.icon);
		this.#isTrashed.setValue(this.#data.isTrashed);
		this.#setVariantAwareValues();
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
		return this.#data?.documentType.icon;
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
		return this.#data?.isTrashed ?? false;
	}

	#setVariantAwareValues() {
		this.#setName();
		this.#setIsDraft();
		this.#setState();
	}

	#setName() {
		const variant = this.#getCurrentVariant();
		if (variant) {
			this.#name.setValue(variant.name);
			return;
		}
		const fallbackName = this.#findVariant(this.#defaultCulture)?.name;
		this.#name.setValue(`(${fallbackName})`);
	}

	#setIsDraft() {
		const variant = this.#getCurrentVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	#setState() {
		const variant = this.#getCurrentVariant();
		const state = variant?.state || UmbDocumentVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	#findVariant(culture: string | undefined) {
		return this.#data?.variants.find((x) => x.culture === culture);
	}

	#getCurrentVariant() {
		if (this.#isInvariant()) {
			return this.#data?.variants?.[0];
		}

		const culture = this.#propertyDataSetCulture?.culture || this.#appCulture;
		return this.#findVariant(culture);
	}

	#isInvariant() {
		return this.#data?.variants?.[0]?.culture === null;
	}
}
