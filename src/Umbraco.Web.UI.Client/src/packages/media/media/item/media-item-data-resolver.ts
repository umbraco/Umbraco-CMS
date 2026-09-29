import type { UmbMediaItemModel, UmbMediaItemVariantModel } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { type UmbVariantContext, UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

type UmbMediaItemDataResolverModel = Omit<UmbMediaItemModel, 'parent' | 'hasChildren'>;

/**
 *
 * @param {Array<UmbMediaItemVariantModel>} variants - An array of variants to check
 * @returns {boolean} Returns true if the variants are invariant, false otherwise
 */
function isVariantsInvariant(variants: Array<UmbMediaItemVariantModel>): boolean {
	return variants?.[0]?.culture === null;
}

/**
 *
 * @param {Array<UmbMediaItemVariantModel>} variants - An array of variants to search
 * @param {string} culture - The culture to find
 * @returns {T | undefined} Returns the variant that matches the culture, or undefined if not found
 */
function findVariant<T extends UmbMediaItemVariantModel>(variants: Array<T>, culture: string): T | undefined {
	return variants.find((x) => x.culture === culture);
}

/**
 * A controller for resolving data for a media item
 * @exports
 * @class UmbMediaItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbMediaItemDataResolver<MediaItemModel extends UmbMediaItemDataResolverModel>
	extends UmbControllerBase
	implements UmbItemDataResolver
{
	#data = new UmbObjectState<MediaItemModel | undefined>(undefined);

	public readonly entityType = this.#data.asObservablePart((x) => x?.entityType);
	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.mediaType.icon);

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

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
			'umbObserveDisplayCulture',
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
	 * Get the current item
	 * @returns {MediaItemModel | undefined} The current item
	 * @memberof UmbMediaItemDataResolver
	 */
	getData(): MediaItemModel | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current item
	 * @param {MediaItemModel | undefined} data The current item
	 * @memberof UmbMediaItemDataResolver
	 */
	setData(data: MediaItemModel | undefined) {
		this.#data.setValue(data);
		this.#setVariantAwareValues();
	}

	/**
	 * Get the entity type of the item
	 * @returns {Promise<string | undefined>} The entity type of the item
	 * @memberof UmbMediaItemDataResolver
	 */
	async getEntityType(): Promise<string | undefined> {
		return await this.observe(this.entityType).asPromise();
	}

	/**
	 * Get the unique of the item
	 * @returns {Promise<string | undefined>} The unique of the item
	 * @memberof UmbMediaItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		return await this.observe(this.unique).asPromise();
	}

	/**
	 * Get the name of the item
	 * @returns {Promise<string>} The name of the item
	 * @memberof UmbMediaItemDataResolver
	 */
	async getName(): Promise<string> {
		return (await this.observe(this.name).asPromise()) || '';
	}

	/**
	 * Get the icon of the item
	 * @returns {Promise<string | undefined>} The icon of the item
	 * @memberof UmbMediaItemDataResolver
	 */
	async getIcon(): Promise<string | undefined> {
		return await this.observe(this.icon).asPromise();
	}

	#setVariantAwareValues() {
		if (!this.#variantContext) return;
		if (!this.#displayCulture) return;
		if (!this.#fallbackCulture) return;
		if (!this.#data) return;
		this.#setName();
	}

	#setName() {
		const variant = this.#getCurrentVariant();
		if (variant?.name) {
			this.#name.setValue(variant.name);
			return;
		}

		const variants = this.getData()?.variants;
		if (variants) {
			const fallbackName = findVariant(variants, this.#fallbackCulture!)?.name ?? variants.find((x) => x.name)?.name;

			if (fallbackName) {
				this.#name.setValue(`(${fallbackName})`);
				return;
			}
		}

		this.#name.setValue('(Untitled)');
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
