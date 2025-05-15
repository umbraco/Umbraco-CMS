import { UmbDocumentVariantState } from '../types.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	observeMultiple,
	UmbBooleanState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { type UmbVariantContext, UMB_VARIANT_CONTEXT, type UmbVariantId } from '@umbraco-cms/backoffice/variant';

type UmbDocumentItemDataResolverModel = Omit<UmbDocumentItemModel, 'parent' | 'hasChildren'>;

/**
 * A controller for resolving data for a document item
 * @exports
 * @class UmbDocumentItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentItemDataResolver<DataType extends UmbDocumentItemDataResolverModel> extends UmbControllerBase {
	#data = new UmbObjectState<DataType | undefined>(undefined);

	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.documentType.icon);
	public readonly isTrashed = this.#data.asObservablePart((x) => x?.isTrashed);

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#state = new UmbStringState(undefined);
	public readonly state = this.#state.asObservable();

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	#variantId?: UmbVariantId;
	#defaultCulture?: string | null;
	#init: Promise<unknown>;
	#variantContext?: UmbVariantContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.#variantContext = context;
			this.#observeVariantContext();
		});
	}

	#observeVariantContext() {
		this.observe(
			observeMultiple([this.#variantContext?.variantId, this.#variantContext?.defaultCulture]),
			([variantId, defaultCulture]) => {
				if (variantId === undefined) return;
				if (defaultCulture === undefined) return;
				this.#variantId = variantId;
				this.#defaultCulture = defaultCulture;
				this.#setVariantAwareValues();
			},
		);
	}

	/**
	 * Get the current item
	 * @returns {DataType | undefined} The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getData(): DataType | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current item
	 * @param {DataType | undefined} data The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	setData(data: DataType | undefined) {
		this.#data.setValue(data);
		this.#setVariantAwareValues();
	}

	/**
	 * Get the unique of the item
	 * @returns {Promise<string | undefined>} The unique of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		return this.#data?.getValue()?.unique;
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
		return this.#data?.getValue()?.documentType.icon;
	}

	/**
	 * Get the state of the item
	 * @returns {Promise<string | undefined>} The state of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getState(): Promise<DocumentVariantStateModel | null | undefined> {
		await this.#init;
		const variant = await this.#getCurrentVariant();
		return variant?.state;
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
		return this.#data?.getValue()?.isTrashed ?? false;
	}

	#setVariantAwareValues() {
		this.#setName();
		this.#setIsDraft();
		this.#setState();
	}

	async #setName() {
		await this.#init;

		const variant = await this.#getCurrentVariant();
		if (variant) {
			this.#name.setValue(variant.name);
			return;
		}

		const fallbackName = this.#findVariant(this.#defaultCulture!)?.name;
		this.#name.setValue(`(${fallbackName})`);
	}

	async #setIsDraft() {
		await this.#init;
		const variant = await this.#getCurrentVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	async #setState() {
		await this.#init;
		const variant = await this.#getCurrentVariant();
		const state = variant?.state || UmbDocumentVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	#findVariant(culture: string | undefined) {
		return this.getData()?.variants.find((x) => x.culture === culture);
	}

	async #getCurrentVariant() {
		await this.#init;
		if (this.#isInvariant()) {
			return this.getData()?.variants?.[0];
		}

		const culture = this.#variantId?.culture;
		return this.#findVariant(culture!);
	}

	#isInvariant() {
		return this.getData()?.variants?.[0]?.culture === null;
	}
}
