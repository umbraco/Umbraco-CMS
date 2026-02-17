import { UmbDocumentVariantState } from '../types.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbArrayState,
	UmbBasicState,
	UmbBooleanState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';
import type { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

type UmbDocumentItemDataResolverModel = Omit<UmbDocumentItemModel, 'parent' | 'hasChildren'>;

/**
 *
 * @param variants
 */
function isVariantsInvariant(variants: Array<{ culture: string | null }>): boolean {
	return variants?.[0]?.culture === null;
}
/**
 *
 * @param variants
 * @param culture
 */
function findVariant<T extends { culture: string | null }>(variants: Array<T>, culture: string): T | undefined {
	return variants.find((x) => x.culture === culture);
}

/**
 * A controller for resolving data for a document item
 * @exports
 * @class UmbDocumentItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentItemDataResolver<DocumentItemModel extends UmbDocumentItemDataResolverModel>
	extends UmbControllerBase
	implements UmbItemDataResolver
{
	#data = new UmbObjectState<DocumentItemModel | undefined>(undefined);

	public readonly entityType = this.#data.asObservablePart((x) => x?.entityType);
	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.documentType.icon);
	public readonly typeUnique = this.#data.asObservablePart((x) => x?.documentType.unique);
	public readonly isTrashed = this.#data.asObservablePart((x) => x?.isTrashed);
	public readonly hasCollection = this.#data.asObservablePart((x) => !!x?.documentType.collection);

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#state = new UmbStringState<DocumentVariantStateModel | null | undefined>(undefined);
	public readonly state = this.#state.asObservable() as Observable<DocumentVariantStateModel | null | undefined>;

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	#createDate = new UmbBasicState<Date | undefined>(undefined);
	public readonly createDate = this.#createDate.asObservable();

	#updateDate = new UmbBasicState<Date | undefined>(undefined);
	public readonly updateDate = this.#updateDate.asObservable();

	#flags = new UmbArrayState<UmbEntityFlag>([], (data) => data.alias);
	public readonly flags = this.#flags.asObservable();

	#variantContext?: UmbVariantContext;
	#fallbackCulture?: string | null;
	#displayCulture?: string | null;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.#variantContext = context;
			this.#observeVariantContext();
		});
	}

	#observeVariantContext() {
		this.observe(
			this.#variantContext?.displayCulture,
			(displayCulture) => {
				if (displayCulture === undefined) return;
				this.#displayCulture = displayCulture;
				this.#setVariantAwareValues();
			},
			'umbObserveVariantId',
		);

		this.observe(
			this.#variantContext?.fallbackCulture,
			(fallbackCulture) => {
				if (fallbackCulture === undefined) return;
				this.#fallbackCulture = fallbackCulture;
				this.#setVariantAwareValues();
			},
			'umbObserveFallbackCulture',
		);
	}

	/**
	 * Get the display culture or fallback culture
	 * @returns {string | null | undefined} The display culture or fallback culture
	 * @memberof UmbDocumentItemDataResolver
	 */
	getCulture(): string | null | undefined {
		return this.#displayCulture || this.#fallbackCulture;
	}

	/**
	 * Get the current item
	 * @returns {DocumentItemModel | undefined} The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getData(): DocumentItemModel | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current item
	 * @param {DocumentItemModel | undefined} data The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	setData(data: DocumentItemModel | undefined) {
		this.#data.setValue(data);
		this.#setVariantAwareValues();
	}

	/**
	 * Get the entity type of the item
	 * @returns {Promise<string | undefined>} The entity type of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getEntityType(): Promise<string | undefined> {
		return await this.observe(this.entityType).asPromise();
	}

	/**
	 * Get the unique of the item
	 * @returns {Promise<string | undefined>} The unique of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		return await this.observe(this.unique).asPromise();
	}

	/**
	 * Get the name of the item
	 * @returns {Promise<string>} The name of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getName(): Promise<string> {
		return (await this.observe(this.name).asPromise()) || '';
	}

	/**
	 * Get the icon of the item
	 * @returns {Promise<string | undefined>} The icon of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIcon(): Promise<string | undefined> {
		return await this.observe(this.icon).asPromise();
	}

	/**
	 * Get the state of the item
	 * @returns {Promise<string | undefined>} The state of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getState(): Promise<DocumentVariantStateModel | null | undefined> {
		return await this.observe(this.state).asPromise();
	}

	/**
	 * Get the isDraft of the item
	 * @returns {Promise<boolean>} The isDraft of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIsDraft(): Promise<boolean> {
		return (await this.observe(this.isDraft).asPromise()) ?? false;
	}

	/**
	 * Get the isTrashed of the item
	 * @returns {Promise<boolean | undefined>} The isTrashed of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIsTrashed(): Promise<boolean> {
		return (await this.observe(this.isTrashed).asPromise()) ?? false;
	}

	/**
	 * Get the create date of the item
	 * @returns {Promise<Date>} The create date of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getCreateDate(): Promise<Date> {
		return (await this.observe(this.createDate).asPromise()) || undefined;
	}

	/**
	 * Get the update date of the item
	 * @returns {Promise<Date>} The update date of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getUpdateDate(): Promise<Date> {
		return (await this.observe(this.updateDate).asPromise()) || undefined;
	}

	/**
	 * Test if the item has a collection
	 * @returns {boolean} Boolean of whether the item has a collection.
	 * @memberof UmbDocumentItemDataResolver
	 */
	getHasCollection(): boolean {
		return this.getData()?.documentType.collection != undefined;
	}

	#setVariantAwareValues() {
		if (!this.#variantContext) return;
		if (!this.#displayCulture) return;
		if (!this.#fallbackCulture) return;
		if (!this.#data) return;
		this.#setName();
		this.#setIsDraft();
		this.#setState();
		this.#setCreateDate();
		this.#setUpdateDate();
		this.#setFlags();
	}

	#setName() {
		const variant = this._getCurrentVariant();
		if (variant?.name) {
			this.#name.setValue(variant.name);
			return;
		}

		const variants = this.getData()?.variants;
		if (variants) {
			// Try fallback culture first, then first variant with any name
			const fallbackName = findVariant(variants, this.#fallbackCulture!)?.name ?? variants.find((x) => x.name)?.name;

			if (fallbackName) {
				this.#name.setValue(`(${fallbackName})`);
				return;
			}
		}

		this.#name.setValue('(Untitled)');
	}

	#setIsDraft() {
		const variant = this._getCurrentVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	#setState() {
		const variant = this._getCurrentVariant();
		const state = variant?.state || UmbDocumentVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	async #setCreateDate() {
		const variant = await this._getCurrentVariant();
		if (variant) {
			this.#createDate.setValue(variant.createDate);
			return;
		}

		const variants = this.getData()?.variants;
		if (variants) {
			const fallbackCreateDate = findVariant(variants, this.#fallbackCulture!)?.createDate;
			this.#createDate.setValue(fallbackCreateDate);
		} else {
			this.#createDate.setValue(undefined);
		}
	}

	async #setUpdateDate() {
		const variant = await this._getCurrentVariant();
		if (variant) {
			this.#updateDate.setValue(variant.updateDate);
			return;
		}

		const variants = this.getData()?.variants;
		if (variants) {
			const fallbackUpdateDate = findVariant(variants, this.#fallbackCulture!)?.updateDate;
			this.#updateDate.setValue(fallbackUpdateDate);
		} else {
			this.#updateDate.setValue(undefined);
		}
	}

	#setFlags() {
		const data = this.getData();
		if (!data) {
			this.#flags.setValue([]);
			return;
		}

		const flags = data.flags ?? [];
		const variantFlags = this._getCurrentVariant()?.flags ?? [];
		this.#flags.setValue([...flags, ...variantFlags]);
	}

	protected _getCurrentVariant() {
		const variants = this.getData()?.variants;
		if (!variants) return undefined;

		if (isVariantsInvariant(variants)) {
			return variants[0];
		}

		return findVariant(variants, this.#displayCulture!);
	}
}
