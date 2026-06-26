import { UMB_VARIANT_CONTEXT } from './context/constants.js';
import type { UmbVariantContext } from './context/variant.context.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbVariantLike {
	culture: string | null;
	segment?: string | null;
}

/**
 * Resolves which variant of a given set matches the ambient variant context.
 * @exports
 * @class UmbVariantResolver
 */
export class UmbVariantResolver<VariantType extends UmbVariantLike = UmbVariantLike> extends UmbControllerBase {
	#variants = new UmbArrayState<VariantType>([], (variant) => `${variant.culture}:${variant.segment ?? null}`);

	#displayCulture = new UmbStringState<string | null | undefined>(undefined);
	public readonly displayCulture = this.#displayCulture.asObservable();

	#fallbackCulture = new UmbStringState<string | null | undefined>(undefined);
	public readonly fallbackCulture = this.#fallbackCulture.asObservable();

	#variant = new UmbObjectState<VariantType | undefined>(undefined);
	public readonly variant = this.#variant.asObservable();
	public readonly culture = this.#variant.asObservablePart((variant) => variant?.culture);

	#fallbackVariant = new UmbObjectState<VariantType | undefined>(undefined);
	public readonly fallbackVariant = this.#fallbackVariant.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.#observeContext(context);
		});
	}

	#observeContext(context: UmbVariantContext | undefined) {
		this.observe(
			context?.displayCulture,
			(displayCulture) => {
				if (displayCulture === undefined) return;
				this.#displayCulture.setValue(displayCulture);
				this.#process();
			},
			'umbObserveDisplayCulture',
		);

		this.observe(
			context?.fallbackCulture,
			(fallbackCulture) => {
				if (fallbackCulture === undefined) return;
				this.#fallbackCulture.setValue(fallbackCulture);
				this.#process();
			},
			'umbObserveFallbackCulture',
		);
	}

	/**
	 * Sets the variants to resolve against the ambient variant context.
	 * @param {Array<VariantType> | undefined} variants - The variants to resolve from.
	 * @memberof UmbVariantResolver
	 */
	setVariants(variants: Array<VariantType> | undefined): void {
		this.#variants.setValue(variants ?? []);
		this.#process();
	}

	/**
	 * Gets the current set of variants.
	 * @returns {Array<VariantType>} The current variants.
	 * @memberof UmbVariantResolver
	 */
	getVariants(): Array<VariantType> {
		return this.#variants.getValue();
	}

	/**
	 * Gets the current display culture.
	 * @returns {string | null | undefined} The display culture.
	 * @memberof UmbVariantResolver
	 */
	getDisplayCulture(): string | null | undefined {
		return this.#displayCulture.getValue();
	}

	/**
	 * Gets the current fallback culture.
	 * @returns {string | null | undefined} The fallback culture.
	 * @memberof UmbVariantResolver
	 */
	getFallbackCulture(): string | null | undefined {
		return this.#fallbackCulture.getValue();
	}

	/**
	 * Gets the variant matching the display culture (or the invariant variant).
	 * @returns {VariantType | undefined} The resolved variant.
	 * @memberof UmbVariantResolver
	 */
	getVariant(): VariantType | undefined {
		return this.#variant.getValue();
	}

	/**
	 * Gets the variant matching the fallback culture.
	 * @returns {VariantType | undefined} The resolved fallback variant.
	 * @memberof UmbVariantResolver
	 */
	getFallbackVariant(): VariantType | undefined {
		return this.#fallbackVariant.getValue();
	}

	#process() {
		const variants = this.#variants.getValue();

		if (variants.length === 0) {
			this.#variant.setValue(undefined);
			this.#fallbackVariant.setValue(undefined);
			return;
		}

		// Invariant content has a single variant with a null culture; it is always the match.
		if (variants[0].culture === null) {
			this.#variant.setValue(variants[0]);
			this.#fallbackVariant.setValue(undefined);
			return;
		}

		const displayCulture = this.#displayCulture.getValue();
		const fallbackCulture = this.#fallbackCulture.getValue();

		this.#variant.setValue(variants.find((variant) => variant.culture === displayCulture));
		this.#fallbackVariant.setValue(variants.find((variant) => variant.culture === fallbackCulture));
	}
}
