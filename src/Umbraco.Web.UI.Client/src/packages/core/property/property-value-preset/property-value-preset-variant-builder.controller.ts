import type { UmbElementValueModel } from '@umbraco-cms/backoffice/content';
import { UmbPropertyValuePresetBuilderController } from './property-value-preset-builder.controller.js';
import type {
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePresetApi,
} from './types.js';
import { UmbVariantId } from '../../variant/variant-id.class.js';

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
		apis: Array<UmbPropertyValuePresetApi>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> {
		let values = [];

		if (propertyType.typeArgs.varyBySegment && propertyType.typeArgs.varyByCulture) {
			if (this.#cultures.length === 0) {
				throw new Error('Cultures must be set when varying by culture.');
			}

			for (const culture of this.#cultures) {
				for (const segment of this.#segments) {
					const value = await this._generatePropertyValue(apis, propertyType, {
						variantId: new UmbVariantId(culture, segment),
					});
					value.culture = culture;
					value.segment = segment;
					values.push(value);
				}
			}
		} else if (propertyType.typeArgs.varyByCulture) {
			if (this.#cultures.length === 0) {
				throw new Error('Cultures must be set when varying by culture.');
			}

			for (const culture of this.#cultures) {
				const value = await this._generatePropertyValue(apis, propertyType, {
					variantId: new UmbVariantId(culture),
				});
				value.culture = culture;
				value.segment = null;
				values.push(value);
			}
		} else if (propertyType.typeArgs.varyBySegment) {
			for (const segment of this.#segments) {
				const value = await this._generatePropertyValue(apis, propertyType, {
					variantId: new UmbVariantId(null, segment),
				});
				// Be aware this maybe should have been the default culture?
				value.culture = null;
				value.segment = segment;
				values.push(value);
			}
		} else {
			const value = await this._generatePropertyValue(apis, propertyType, {});
			// Be aware this maybe should have been the default culture?
			value.culture = null;
			value.segment = null;
			values.push(value);
		}
		return values;
	}
}
