import type { UmbPropertyValueData } from '../types.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A lookup over a list of property values, by alias and optionally by variant.
 *
 * The list is indexed once on construction, so lookups do not scan it. The index does not observe
 * the list: build a new index when the list changes.
 *
 * When several values share the same alias (and variant), the first one in the list is returned.
 * Values without a culture or segment are treated as invariant.
 */
export class UmbPropertyValueIndex<ValueType extends UmbPropertyValueData = UmbPropertyValueData> {
	readonly #firstByAlias = new Map<string, ValueType>();
	readonly #firstByAliasAndVariant = new Map<string, Map<string, ValueType>>();

	/**
	 * @param {Array<UmbPropertyValueData>} values - The values to index.
	 */
	constructor(values: ReadonlyArray<ValueType>) {
		for (const value of values) {
			if (!this.#firstByAlias.has(value.alias)) {
				this.#firstByAlias.set(value.alias, value);
			}

			const { culture, segment } = value as { culture?: string | null; segment?: string | null };
			const variantKey = UmbPropertyValueIndex.#variantKey(new UmbVariantId(culture, segment));
			let byVariant = this.#firstByAliasAndVariant.get(value.alias);
			if (!byVariant) {
				byVariant = new Map();
				this.#firstByAliasAndVariant.set(value.alias, byVariant);
			}
			if (!byVariant.has(variantKey)) {
				byVariant.set(variantKey, value);
			}
		}
	}

	/**
	 * Finds the first value with the given alias, and when a variant is given, the same variant.
	 * @param {string} alias - The property alias.
	 * @param {UmbVariantId} [variantId] - The variant to match. When omitted, the variant of the values is ignored.
	 * @returns {UmbPropertyValueData | undefined} The first matching value, if any.
	 */
	find(alias: string, variantId?: UmbVariantId): ValueType | undefined {
		if (!variantId) {
			return this.#firstByAlias.get(alias);
		}
		return this.#firstByAliasAndVariant.get(alias)?.get(UmbPropertyValueIndex.#variantKey(variantId));
	}

	static #variantKey(variantId: UmbVariantId): string {
		return JSON.stringify([variantId.culture, variantId.segment]);
	}
}
