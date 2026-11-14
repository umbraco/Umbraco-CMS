import type { UmbPropertyValueData } from '../types.js';
import { UmbVariantId, type UmbObjectWithVariantProperties } from '@umbraco-cms/backoffice/variant';

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

			const variantKey = UmbVariantId.ToString(value as Partial<UmbObjectWithVariantProperties>);
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
	 * @param {Partial<UmbObjectWithVariantProperties>} [variant] - The culture and segment to match, such as a Variant Id or a value. Omitted values are treated as invariant. When the argument is omitted, the variant of the values is ignored.
	 * @returns {UmbPropertyValueData | undefined} The first matching value, if any.
	 */
	find(alias: string, variant?: Partial<UmbObjectWithVariantProperties>): ValueType | undefined {
		if (!variant) {
			return this.#firstByAlias.get(alias);
		}
		return this.#firstByAliasAndVariant.get(alias)?.get(UmbVariantId.ToString(variant));
	}
}
