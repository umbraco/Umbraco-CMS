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
	variantId = this.#variantId.asObservable();

	#culture = new UmbStringState<string | null | undefined>(undefined);
	culture = this.#culture.asObservable();

	#segment = new UmbStringState<string | null | undefined>(undefined);
	segment = this.#segment.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_VARIANT_CONTEXT);
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
}
