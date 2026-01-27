import { UmbElementVariantState } from '../../types.js';
import type { UmbElementItemModel } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import {
	UmbArrayState,
	UmbBasicState,
	UmbBooleanState,
	UmbObjectState,
	UmbStringState,
	type Observable,
} from '@umbraco-cms/backoffice/observable-api';
import { type UmbVariantContext, UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

type UmbElementItemDataResolverModel = Omit<UmbElementItemModel, 'parent' | 'hasChildren'>;

/**
 * @param variants
 * @returns {boolean}
 */
function isVariantsInvariant(variants: Array<{ culture: string | null }>): boolean {
	return variants?.[0]?.culture === null;
}

/**
 *
 * @param variants
 * @param culture
 * @returns {T | undefined}
 */
function findVariant<T extends { culture: string | null }>(variants: Array<T>, culture: string): T | undefined {
	return variants.find((x) => x.culture === culture);
}

/**
 * A controller for resolving data for a element item
 * @exports
 * @class UmbElementItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbElementItemDataResolver<ElementItemModel extends UmbElementItemDataResolverModel>
	extends UmbControllerBase
	implements UmbItemDataResolver
{
	#data = new UmbObjectState<ElementItemModel | undefined>(undefined);

	public readonly entityType = this.#data.asObservablePart((x) => x?.entityType);
	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.documentType.icon);
	public readonly typeUnique = this.#data.asObservablePart((x) => x?.documentType.unique);
	public readonly isTrashed = this.#data.asObservablePart((x) => x?.isTrashed);
	public readonly hasCollection = this.#data.asObservablePart((x) => !!x?.documentType.collection);

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#state = new UmbStringState<UmbElementVariantState | null | undefined>(undefined);
	public readonly state = this.#state.asObservable() as Observable<UmbElementVariantState | null | undefined>;

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
	 * @memberof UmbElementItemDataResolver
	 */
	getCulture(): string | null | undefined {
		return this.#displayCulture || this.#fallbackCulture;
	}

	/**
	 * Get the current item
	 * @returns {ElementItemModel | undefined} The current item
	 * @memberof UmbElementItemDataResolver
	 */
	getData(): ElementItemModel | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current item
	 * @param {ElementItemModel | undefined} data The current item
	 * @memberof UmbElementItemDataResolver
	 */
	setData(data: ElementItemModel | undefined) {
		this.#data.setValue(data);
		this.#setVariantAwareValues();
	}

	/**
	 * Get the entity type of the item
	 * @returns {Promise<string | undefined>} The entity type of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getEntityType(): Promise<string | undefined> {
		return await this.observe(this.entityType).asPromise();
	}

	/**
	 * Get the unique of the item
	 * @returns {Promise<string | undefined>} The unique of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		return await this.observe(this.unique).asPromise();
	}

	/**
	 * Get the name of the item
	 * @returns {Promise<string>} The name of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getName(): Promise<string> {
		return (await this.observe(this.name).asPromise()) || '';
	}

	/**
	 * Get the icon of the item
	 * @returns {Promise<string | undefined>} The icon of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getIcon(): Promise<string | undefined> {
		return await this.observe(this.icon).asPromise();
	}

	/**
	 * Get the state of the item
	 * @returns {Promise<string | undefined>} The state of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getState(): Promise<UmbElementVariantState | null | undefined> {
		return await this.observe(this.state).asPromise();
	}

	/**
	 * Get the isDraft of the item
	 * @returns {Promise<boolean>} The isDraft of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getIsDraft(): Promise<boolean> {
		return (await this.observe(this.isDraft).asPromise()) ?? false;
	}

	/**
	 * Get the isTrashed of the item
	 * @returns {Promise<boolean | undefined>} The isTrashed of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getIsTrashed(): Promise<boolean> {
		return (await this.observe(this.isTrashed).asPromise()) ?? false;
	}

	/**
	 * Get the create date of the item
	 * @returns {Promise<Date>} The create date of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getCreateDate(): Promise<Date> {
		return (await this.observe(this.createDate).asPromise()) || undefined;
	}

	/**
	 * Get the update date of the item
	 * @returns {Promise<Date>} The update date of the item
	 * @memberof UmbElementItemDataResolver
	 */
	async getUpdateDate(): Promise<Date> {
		return (await this.observe(this.updateDate).asPromise()) || undefined;
	}

	/**
	 * Test if the item has a collection
	 * @returns {boolean} Boolean of whether the item has a collection.
	 * @memberof UmbElementItemDataResolver
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
		const variant = this.#getCurrentVariant();
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
		const variant = this.#getCurrentVariant();
		const isDraft = variant?.state === UmbElementVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	#setState() {
		const variant = this.#getCurrentVariant();
		const state = variant?.state || UmbElementVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	async #setCreateDate() {
		const variant = await this.#getCurrentVariant();
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
		const variant = await this.#getCurrentVariant();
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
		const variantFlags = this.#getCurrentVariant()?.flags ?? [];
		this.#flags.setValue([...flags, ...variantFlags]);
	}

	#getCurrentVariant() {
		const variants = this.getData()?.variants;
		if (!variants) return undefined;

		if (isVariantsInvariant(variants)) {
			return variants[0];
		}

		return findVariant(variants, this.#displayCulture!);
	}
}
