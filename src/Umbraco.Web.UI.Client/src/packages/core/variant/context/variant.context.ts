import { UmbVariantId } from '../variant-id.class.js';
import { UMB_VARIANT_CONTEXT } from './variant.context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbClassState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

/**
 * A context for the current variant state.
 * @class UmbVariantContext
 * @augments {UmbContextBase}
 * @implements {UmbVariantContext}
 */
export class UmbVariantContext extends UmbContextBase {
	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();
	public readonly culture = this.#variantId.asObservablePart((x) => x?.culture);
	public readonly segment = this.#variantId.asObservablePart((x) => x?.segment);

	#fallbackCulture = new UmbStringState<string | null | undefined>(undefined);
	public fallbackCulture = this.#fallbackCulture.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_VARIANT_CONTEXT);

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.observe(
				context?.fallbackCulture,
				(fallbackCulture) => {
					if (!fallbackCulture) return;
					this.setFallbackCulture(fallbackCulture);
				},
				'observeFallbackCulture',
			);
		}).skipHost();
	}

	/**
	 * Sets the variant id state
	 * @param {UmbVariantId | undefined} variantId - The variant to set
	 * @memberof UmbVariantContext
	 */
	async setVariantId(variantId: UmbVariantId | undefined): Promise<void> {
		this.#variantId.setValue(variantId);
	}

	/**
	 * Gets variant state
	 * @returns {Promise<UmbVariantId | undefined>} - The variant state
	 * @memberof UmbVariantContext
	 */
	async getVariantId(): Promise<UmbVariantId | undefined> {
		return this.#variantId.getValue();
	}

	/**
	 * Gets the culture state
	 * @returns {(Promise<string | null | undefined>)} - The culture state
	 * @memberof UmbVariantContext
	 */
	async getCulture(): Promise<string | null | undefined> {
		return this.#variantId.getValue()?.culture;
	}

	/**
	 * Sets the variant culture state
	 * @param {string | undefined} culture - The culture to set
	 * @memberof UmbVariantContext
	 */
	async setCulture(culture: string | null): Promise<void> {
		const variantId = new UmbVariantId(culture, this.#variantId.getValue()?.segment);
		this.#variantId.setValue(variantId);
	}

	/**
	 * Gets the variant segment state
	 * @returns {(Promise<string | null | undefined>)} - The segment state
	 * @memberof UmbVariantContext
	 */
	async getSegment(): Promise<string | null | undefined> {
		return this.#variantId.getValue()?.segment;
	}

	/**
	 * Sets the variant segment state
	 * @param {string | undefined} segment - The segment to set
	 * @memberof UmbVariantContext
	 */
	async setSegment(segment: string | null): Promise<void> {
		const variantId = new UmbVariantId(this.#variantId.getValue()?.culture, segment);
		this.#variantId.setValue(variantId);
	}

	/**
	 * Gets the fallback culture state
	 * @returns {(Promise<string | null | undefined>)} - The fallback culture state
	 * @memberof UmbVariantContext
	 */
	async getFallbackCulture(): Promise<string | null | undefined> {
		return this.#fallbackCulture.getValue();
	}

	/**
	 * Sets the fallback culture state
	 * @param {string | undefined} culture - The fallback culture to set
	 * @memberof UmbVariantContext
	 */
	async setFallbackCulture(culture: string | null): Promise<void> {
		this.#fallbackCulture.setValue(culture);
	}
}
