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
	public variantId = this.#variantId.asObservable();

	#culture = new UmbStringState<string | null | undefined>(undefined);
	public culture = this.#culture.asObservable();

	#segment = new UmbStringState<string | null | undefined>(undefined);
	public segment = this.#segment.asObservable();

	#defaultCulture = new UmbStringState<string | null | undefined>(undefined);
	public defaultCulture = this.#defaultCulture.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_VARIANT_CONTEXT);

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.observe(context?.defaultCulture, (defaultCulture) => {
				if (!defaultCulture) return;
				this.setDefaultCulture(defaultCulture);
			});
		}).skipHost();
	}

	/**
	 * Sets the variant id state
	 * @param {UmbVariantId | undefined} variantId - The variant to set
	 * @memberof UmbVariantContext
	 */
	setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
		this.#culture.setValue(variantId?.culture);
		this.#segment.setValue(variantId?.segment);
	}

	/**
	 * Gets variant state
	 * @returns {UmbVariantId | undefined} - The variant state
	 * @memberof UmbVariantContext
	 */
	getVariantId(): UmbVariantId | undefined {
		return this.#variantId.getValue();
	}

	/**
	 * Gets the culture state
	 * @returns {(string | null | undefined)} - The culture state
	 * @memberof UmbVariantContext
	 */
	getCulture(): string | null | undefined {
		return this.#variantId.getValue()?.culture;
	}

	/**
	 * Sets the variant culture state
	 * @param {string | undefined} culture - The culture to set
	 * @memberof UmbVariantContext
	 */
	setCulture(culture: string | null): void {
		this.#culture.setValue(culture);
		const variantId = new UmbVariantId(culture, this.#segment.getValue());
		this.#variantId.setValue(variantId);
	}

	/**
	 * Gets the variant segment state
	 * @returns {(string | null | undefined)} - The segment state
	 * @memberof UmbVariantContext
	 */
	getSegment(): string | null | undefined {
		return this.#variantId.getValue()?.segment;
	}

	/**
	 * Sets the variant segment state
	 * @param {string | undefined} segment - The segment to set
	 * @memberof UmbVariantContext
	 */
	setSegment(segment: string | null): void {
		this.#segment.setValue(segment);
		const variantId = new UmbVariantId(this.#culture.getValue(), segment);
		this.#variantId.setValue(variantId);
	}

	/**
	 * Gets the default culture state
	 * @returns {(string | null | undefined)} - The default culture state
	 * @memberof UmbVariantContext
	 */
	getDefaultCulture(): string | null | undefined {
		return this.#defaultCulture.getValue();
	}

	/**
	 * Sets the default culture state
	 * @param {string | undefined} culture - The default culture to set
	 * @memberof UmbVariantContext
	 */
	setDefaultCulture(culture: string | null): void {
		this.#defaultCulture.setValue(culture);
	}
}
