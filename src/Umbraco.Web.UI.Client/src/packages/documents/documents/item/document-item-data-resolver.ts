import { UmbDocumentVariantState } from '../variant-state.js';
import type { UmbDocumentItemModel, UmbDocumentItemVariantModel } from './types.js';
import {
	UmbArrayState,
	UmbBasicState,
	UmbBooleanState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import { UmbVariantResolver } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

type UmbDocumentItemDataResolverModel = Omit<UmbDocumentItemModel, 'parent' | 'hasChildren'>;

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

	#state = new UmbStringState<UmbDocumentVariantState | null | undefined>(undefined);
	public readonly state = this.#state.asObservable() as Observable<UmbDocumentVariantState | null | undefined>;

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	#createDate = new UmbBasicState<Date | undefined>(undefined);
	public readonly createDate = this.#createDate.asObservable();

	#updateDate = new UmbBasicState<Date | undefined>(undefined);
	public readonly updateDate = this.#updateDate.asObservable();

	#flags = new UmbArrayState<UmbEntityFlag>([], (data) => data.alias);
	public readonly flags = this.#flags.asObservable();

	#variantResolver: UmbVariantResolver<UmbDocumentItemVariantModel>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#variantResolver = new UmbVariantResolver<UmbDocumentItemVariantModel>(this);

		// Recompute when either the ambient culture or the resolved variant changes. Observing the cultures
		// triggers a recompute when a culture arrives even if the matched variant is unchanged (clearing the
		// guard below); observing the variants ensures the recompute reads the freshly resolved variant.
		this.observe(this.#variantResolver.displayCulture, () => this.#setVariantAwareValues(), 'umbObserveDisplayCulture');
		this.observe(
			this.#variantResolver.fallbackCulture,
			() => this.#setVariantAwareValues(),
			'umbObserveFallbackCulture',
		);
		this.observe(this.#variantResolver.variant, () => this.#setVariantAwareValues(), 'umbObserveVariant');
		this.observe(
			this.#variantResolver.fallbackVariant,
			() => this.#setVariantAwareValues(),
			'umbObserveFallbackVariant',
		);
	}

	/**
	 * Get the display culture or fallback culture
	 * @returns {string | null | undefined} The display culture or fallback culture
	 * @memberof UmbDocumentItemDataResolver
	 */
	getCulture(): string | null | undefined {
		return this.#variantResolver.getDisplayCulture() || this.#variantResolver.getFallbackCulture();
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
		this.#variantResolver.setVariants(data?.variants);
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
	async getState(): Promise<UmbDocumentVariantState | null | undefined> {
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
		if (!this.#variantResolver.getDisplayCulture()) return;
		if (!this.#variantResolver.getFallbackCulture()) return;
		if (!this.getData()) return;
		this.#setName();
		this.#setIsDraft();
		this.#setState();
		this.#setCreateDate();
		this.#setUpdateDate();
		this.#setFlags();
	}

	#setName() {
		const variant = this.#variantResolver.getVariant();
		if (variant?.name) {
			this.#name.setValue(variant.name);
			return;
		}

		// Try fallback culture first, then first variant with any name
		const fallbackName =
			this.#variantResolver.getFallbackVariant()?.name ?? this.#variantResolver.getVariants().find((x) => x.name)?.name;

		if (fallbackName) {
			this.#name.setValue(`(${fallbackName})`);
			return;
		}

		this.#name.setValue('(Untitled)');
	}

	#setIsDraft() {
		const variant = this.#variantResolver.getVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	#setState() {
		const variant = this.#variantResolver.getVariant();
		const state = variant?.state || UmbDocumentVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	#setCreateDate() {
		const variant = this.#variantResolver.getVariant();
		this.#createDate.setValue((variant ?? this.#variantResolver.getFallbackVariant())?.createDate);
	}

	#setUpdateDate() {
		const variant = this.#variantResolver.getVariant();
		this.#updateDate.setValue((variant ?? this.#variantResolver.getFallbackVariant())?.updateDate);
	}

	#setFlags() {
		const data = this.getData();
		if (!data) {
			this.#flags.setValue([]);
			return;
		}

		const flags = data.flags ?? [];
		const variantFlags = this.#variantResolver.getVariant()?.flags ?? [];
		this.#flags.setValue([...flags, ...variantFlags]);
	}
}
