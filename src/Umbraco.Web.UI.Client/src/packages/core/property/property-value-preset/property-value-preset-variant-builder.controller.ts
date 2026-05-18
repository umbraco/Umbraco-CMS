import { UmbVariantId } from '../../variant/variant-id.class.js';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';
import type {
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
	UmbPropertyValuePresetApiCallArgs,
} from './types.js';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

type ReturnType = UmbElementValueModel;

export class UmbPropertyValuePresetVariantBuilderController extends UmbPropertyValuePresetBuilderController<ReturnType> {
	#cultures: Array<null | string> = [];
	// Always declare the default segment (null)
	#segments: Array<null | string> = [null];
	#variantOptions?: Array<UmbVariantId>;

	/**
	 * Sets the cultures to generate values for.
	 * Cannot be used together with setVariantOptions.
	 * @param {Array<string>} cultures - Array of culture codes (e.g., ['en-US', 'da-DK'])
	 * @deprecated Use setVariantOptions instead for more fine-grained control.
	 */
	setCultures(cultures: Array<string>): void {
		new UmbDeprecation({
			removeInVersion: '20.0.0',
			deprecated: 'setCultures',
			solution: 'Use .setVariantOptions method instead',
		}).warn();

		if (this.#variantOptions) {
			throw new Error('setCultures cannot be used together with setVariantOptions.');
		}
		this.#cultures = cultures;
	}

	/**
	 * Sets the segments to generate values for.
	 * Cannot be used together with setVariantOptions.
	 * @param {Array<string>} segments - Array of segment aliases
	 * @deprecated Use setVariantOptions instead for more fine-grained control.
	 */
	setSegments(segments: Array<string>): void {
		new UmbDeprecation({
			removeInVersion: '20.0.0',
			deprecated: 'setSegments',
			solution: 'Use .setVariantOptions method instead',
		}).warn();

		if (this.#variantOptions) {
			throw new Error('setSegments cannot be used together with setVariantOptions.');
		}
		// No matter how many segments are present, always include the default segment (null)
		this.#segments = [null, ...segments];
	}

	/**
	 * Sets explicit variant options (culture + segment combinations) to generate values for.
	 * This allows fine-grained control over which culture/segment combinations are valid,
	 * supporting scenarios where segments are culture-specific.
	 * Cannot be used together with setCultures or setSegments.
	 * @param {Array<UmbVariantId>} options - Array of UmbVariantId instances representing valid culture/segment combinations
	 */
	setVariantOptions(options: Array<UmbVariantId>): void {
		if (this.#cultures.length > 0 || this.#segments.length > 1) {
			throw new Error('setVariantOptions cannot be used together with setCultures or setSegments.');
		}
		this.#variantOptions = options;
	}

	protected override async _generatePropertyValues(
		apis: Array<UmbPropertyValuePreset>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> {
		// Determine which variant options to use based on the configuration method
		const variantOptionsToUse = this.#variantOptions
			? this.#getFilteredVariantOptions(propertyType)
			: this.#buildVariantOptionsFromCulturesAndSegments(propertyType);

		// Generate values for each variant option
		const values: Array<ReturnType> = [];
		for (const variantId of variantOptionsToUse) {
			const value = await this._generatePropertyValue(
				apis,
				propertyType,
				this.#makeArgsFor(propertyType.alias, variantId.culture, variantId.segment),
			);
			if (value) {
				// Reconstruct with consistent key ordering (editorAlias, culture, segment, alias, value)
				// to match the data source format. JSON.stringify respects insertion order, so mismatched
				// key ordering causes false-positive pending change detection.
				values.push({
					editorAlias: value.editorAlias,
					culture: variantId.culture,
					segment: variantId.segment,
					alias: value.alias,
					value: value.value,
				} as ReturnType);
			}
		}
		return values;
	}

	/**
	 * Derives the variant ids to generate values for, from the variant options supplied via
	 * {@link setVariantOptions}. Those options describe the document's variants (driven by the
	 * content type's variance: e.g. one entry per culture, or per culture × segment), which
	 * may not match how the property itself varies.
	 *
	 * Each option is projected onto the property's variance — the dimensions the property
	 * varies on (culture and/or segment) are kept, the rest are collapsed to null — and the
	 * projected ids are deduped. A culture-invariant option is dropped when the property
	 * varies by culture, because there is no culture to hoist it to.
	 * @param {UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel} propertyType - Property type model, whose variance drives the projection
	 * @returns {Array<UmbVariantId>} The variant ids to generate values for
	 */
	#getFilteredVariantOptions(
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Array<UmbVariantId> {
		if (!this.#variantOptions) {
			return [];
		}

		const variesByCulture = propertyType.typeArgs.variesByCulture ?? false;
		const variesBySegment = propertyType.typeArgs.variesBySegment ?? false;

		if (variesByCulture && !this.#variantOptions.some((v) => v.culture !== null)) {
			throw new Error('Cultures must be set when varying by culture.');
		}

		const seen = new Set<string>();
		const result: Array<UmbVariantId> = [];
		for (const variantId of this.#variantOptions) {
			if (variesByCulture && variantId.culture === null) {
				continue;
			}
			const projected = variantId.toVariant(variesByCulture, variesBySegment);
			const key = projected.toString();
			if (seen.has(key)) {
				continue;
			}
			seen.add(key);
			result.push(projected);
		}

		return result;
	}

	/**
	 * Builds variant options from the legacy setCultures/setSegments approach.
	 * This maintains backward compatibility with existing usage.
	 * @param {UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel} propertyType - Property type model
	 * @returns {Array<UmbVariantId>} Array of UmbVariantId instances representing culture/segment combinations
	 */
	#buildVariantOptionsFromCulturesAndSegments(
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Array<UmbVariantId> {
		const variesByCulture = propertyType.typeArgs.variesByCulture;
		const variesBySegment = propertyType.typeArgs.variesBySegment;

		// Validate that cultures are set when property varies by culture
		if (variesByCulture && this.#cultures.length === 0) {
			throw new Error('Cultures must be set when varying by culture.');
		}

		const options: Array<UmbVariantId> = [];

		if (variesByCulture && variesBySegment) {
			// Cartesian product of cultures × segments
			for (const culture of this.#cultures) {
				for (const segment of this.#segments) {
					options.push(new UmbVariantId(culture, segment));
				}
			}
		} else if (variesByCulture) {
			// Only cultures
			for (const culture of this.#cultures) {
				options.push(new UmbVariantId(culture, null));
			}
		} else if (variesBySegment) {
			// Only segments (culture invariant)
			for (const segment of this.#segments) {
				options.push(new UmbVariantId(null, segment));
			}
		} else {
			// Invariant
			options.push(new UmbVariantId(null, null));
		}

		return options;
	}

	#makeArgsFor(alias: string, culture: null | string, segment: null | string) {
		const variantId = new UmbVariantId(culture, segment);
		const args: Partial<UmbPropertyValuePresetApiCallArgs> = {
			variantId,
			value: this.#findExistingValue(alias, variantId),
		};
		return args;
	}

	#findExistingValue(alias: string, variantId: UmbVariantId): unknown {
		if (!this._existingValues) {
			return undefined;
		}

		const exactMatch = this._existingValues.find((x) => x.alias === alias && variantId.compare(x));
		if (exactMatch) {
			return exactMatch.value;
		}
		return undefined;
	}
}
