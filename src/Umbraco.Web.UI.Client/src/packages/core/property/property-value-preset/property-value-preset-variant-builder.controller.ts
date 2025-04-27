import { UmbVariantId } from '../../variant/variant-id.class.js';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';
import type {
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
} from './types.js';
import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';

type ReturnType = UmbElementValueModel;

export class UmbPropertyValuePresetVariantBuilderController extends UmbPropertyValuePresetBuilderController<ReturnType> {
	#cultures: Array<null | string> = [];
	// Always declare the default segment (null)
	#segments: Array<null | string> = [null];

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
					const value = await this._generatePropertyValue(apis, propertyType, {
						variantId: new UmbVariantId(culture, segment),
					});
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
				const value = await this._generatePropertyValue(apis, propertyType, {
					variantId: new UmbVariantId(culture),
				});
				if (value) {
					value.culture = culture;
					value.segment = null;
					values.push(value);
				}
			}
		} else if (propertyType.typeArgs.variesBySegment) {
			for (const segment of this.#segments) {
				const value = await this._generatePropertyValue(apis, propertyType, {
					variantId: new UmbVariantId(null, segment),
				});
				if (value) {
					// Be aware this maybe should have been the default culture?
					value.culture = null;
					value.segment = segment;
					values.push(value);
				}
			}
		} else {
			const value = await this._generatePropertyValue(apis, propertyType, {});
			if (value) {
				// Be aware this maybe should have been the default culture?
				value.culture = null;
				value.segment = null;
				values.push(value);
			}
		}
		return values;
	}
}
