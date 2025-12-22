import { UmbVariantId } from '../../variant/variant-id.class.js';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';
import type {
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
	UmbPropertyValuePresetApiCallArgs,
} from './types.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

type ReturnType = UmbElementValueModel;

export class UmbPropertyValuePresetVariantBuilderController extends UmbPropertyValuePresetBuilderController<ReturnType> {
	#cultures: Array<null | string> = [];
	// Always declare the default segment (null)
	#segments: Array<null | string> = [null];

	// TODO: We properly need to break this, as Engage needs to control which Segments are available for each culture, maybe investigate the option to go about a new option to just parse options.? Break in v.17.0 [NL]

	setCultures(cultures: Array<string>): void {
		this.#cultures = cultures;
	}
	setSegments(segments: Array<string>): void {
		// No matter how many segments are present, always include the default segment (null)
		this.#segments = [null, ...segments];
	}

	protected override async _generatePropertyValues(
		apis: Array<UmbPropertyValuePreset>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> {
		const values: Array<ReturnType> = [];

		if (propertyType.typeArgs.variesBySegment && propertyType.typeArgs.variesByCulture) {
			if (this.#cultures.length === 0) {
				throw new Error('Cultures must be set when varying by culture.');
			}

			for (const culture of this.#cultures) {
				for (const segment of this.#segments) {
					const value = await this._generatePropertyValue(
						apis,
						propertyType,
						this.#makeArgsFor(propertyType.alias, culture, segment),
					);
					if (value) {
						value.culture = culture;
						value.segment = segment;
						values.push(value);
					}
				}
			}
		} else if (propertyType.typeArgs.variesByCulture) {
			if (this.#cultures.length === 0) {
				throw new Error('Cultures must be set when varying by culture.');
			}

			for (const culture of this.#cultures) {
				const value = await this._generatePropertyValue(
					apis,
					propertyType,
					this.#makeArgsFor(propertyType.alias, culture, null),
				);
				if (value) {
					value.culture = culture;
					value.segment = null;
					values.push(value);
				}
			}
		} else if (propertyType.typeArgs.variesBySegment) {
			for (const segment of this.#segments) {
				const value = await this._generatePropertyValue(
					apis,
					propertyType,
					this.#makeArgsFor(propertyType.alias, null, segment),
				);
				if (value) {
					// Be aware this maybe should have been the default culture?
					value.culture = null;
					value.segment = segment;
					values.push(value);
				}
			}
		} else {
			const value = await this._generatePropertyValue(
				apis,
				propertyType,
				this.#makeArgsFor(propertyType.alias, null, null),
			);
			if (value) {
				// Be aware this maybe should have been the default culture?
				value.culture = null;
				value.segment = null;
				values.push(value);
			}
		}
		return values;
	}

	#makeArgsFor(alias: string, culture: null | string, segment: null | string) {
		const variantId = new UmbVariantId(culture, segment);
		const args: Partial<UmbPropertyValuePresetApiCallArgs> = {
			variantId,
			value: this.#findExistingValue(alias, variantId),
		};
		return args;
	}

	/**
	 * Finds an existing value for a property, with fallback logic for variation setting changes.
	 * When a property changes from invariant to variant (or vice versa), the existing values
	 * may have a different culture/segment than what we're looking for. This method handles
	 * value migration by trying fallback lookups.
	 */
	#findExistingValue(alias: string, variantId: UmbVariantId): unknown {
		if (!this._existingValues) return undefined;

		// First, try exact match (same alias + same culture/segment)
		const exactMatch = this._existingValues.find((x) => x.alias === alias && variantId.compare(x));
		if (exactMatch) {
			return exactMatch.value;
		}

		// No exact match - try fallback for variation setting changes
		// Get all values for this property alias
		const valuesForAlias = this._existingValues.filter((x) => x.alias === alias);
		if (valuesForAlias.length === 0) {
			return undefined;
		}

		// Fallback 1: If looking for a culture-specific value, try to use the invariant value
		// This handles: invariant → variant migration
		if (variantId.culture !== null) {
			const invariantValue = valuesForAlias.find((x) => x.culture === null && x.segment === variantId.segment);
			if (invariantValue) {
				return invariantValue.value;
			}
		}

		// Fallback 2: If looking for an invariant value, try to use the first culture-specific value
		// This handles: variant → invariant migration
		if (variantId.culture === null) {
			const anyVariantValue = valuesForAlias.find((x) => x.culture !== null && x.segment === variantId.segment);
			if (anyVariantValue) {
				return anyVariantValue.value;
			}
		}

		return undefined;
	}
}
