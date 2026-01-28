import type { UmbContentDetailModel, UmbContentValueModel } from '../index.js';
import type { UmbContentDetailWorkspaceContextBase } from './content-detail-workspace-base.js';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

/**
 * @class UmbContentDetailWorkspaceTypeTransformController
 * @description - Controller to handle content detail workspace type transformations, such as property variation changes.
 */
export class UmbContentDetailWorkspaceTypeTransformController<
	DetailModelType extends UmbContentDetailModel<UmbEntityVariantModel>,
> extends UmbControllerBase {
	#workspace: UmbContentDetailWorkspaceContextBase<DetailModelType, any, any, any>;
	// Current property types, currently used to detect variation changes:
	#propertyTypes?: Array<UmbPropertyTypeModel>;

	constructor(host: UmbContentDetailWorkspaceContextBase<DetailModelType, any, any, any>) {
		super(host);

		this.#workspace = host;

		// Observe property variation changes to trigger value migration when properties change
		// from invariant to variant (or vice versa) via Infinite Editing
		this.observe(
			host.structure.contentTypeProperties,
			(propertyTypes: Array<UmbPropertyTypeModel>) => {
				this.#handlePropertyTypeVariationChanges(this.#propertyTypes, propertyTypes);
				this.#propertyTypes = propertyTypes;
			},
			null,
		);
	}

	async #handlePropertyTypeVariationChanges(
		oldPropertyTypes: Array<UmbPropertyTypeModel> | undefined,
		newPropertyTypes: Array<UmbPropertyTypeModel> | undefined,
	): Promise<void> {
		if (!oldPropertyTypes || !newPropertyTypes) {
			return;
		}
		// Skip if no current data or if this is initial load
		const currentData = this.#workspace.getData();
		if (!currentData) {
			return;
		}

		const defaultLanguage = this.#getDefaultLanguage();
		const result = this.#transformValuesForVariationChanges(
			currentData.values,
			oldPropertyTypes,
			newPropertyTypes,
			defaultLanguage,
		);

		if (result.hasChanges) {
			this.#workspace.setData({ ...currentData, values: result.values });
		}
	}

	#getDefaultLanguage(): string {
		const languages = this.#workspace.getLanguages();
		const defaultLanguage = languages.find((lang) => lang.isDefault)?.unique;
		if (!defaultLanguage) {
			throw new Error('Default language not found');
		}
		return defaultLanguage;
	}

	/**
	 * Transforms all values based on property variation changes.
	 * Handles both invariant→variant and variant→invariant transitions, including cleanup of duplicate values.
	 * When transitioning to invariant, keeps all segment values for the chosen culture (default language preferred).
	 * @param {Array<UmbContentValueModel>} values - All content values
	 * @param {Array<UmbPropertyTypeModel>} oldPropertyTypes - Previous property type definitions
	 * @param {Array<UmbPropertyTypeModel>} newPropertyTypes - New property type definitions
	 * @param {string} defaultLanguage - The default language code
	 * @returns {{ values: Array<UmbContentValueModel>; hasChanges: boolean }} Transformed values and change flag
	 */
	#transformValuesForVariationChanges(
		values: Array<UmbContentValueModel>,
		oldPropertyTypes: Array<UmbPropertyTypeModel>,
		newPropertyTypes: Array<UmbPropertyTypeModel>,
		defaultLanguage: string,
	): { values: Array<UmbContentValueModel>; hasChanges: boolean } {
		// Group values by property alias for efficient processing
		const valuesByAlias = new Map<string, Array<UmbContentValueModel>>();
		for (const value of values) {
			const existing = valuesByAlias.get(value.alias) ?? [];
			existing.push(value);
			valuesByAlias.set(value.alias, existing);
		}

		const result: Array<UmbContentValueModel> = [];
		let hasChanges = false;

		for (const [alias, valuesOfAlias] of valuesByAlias) {
			const oldType = oldPropertyTypes.find((p) => p.alias === alias);
			const newType = newPropertyTypes.find((p) => p.alias === alias);

			// If we can't find both types, keep values unchanged (composition may not have been loaded yet)
			if (!oldType || !newType) {
				result.push(...valuesOfAlias);
				continue;
			}

			// No variation change - keep all values as-is
			if (oldType.variesByCulture === newType.variesByCulture) {
				result.push(...valuesOfAlias);
				continue;
			}

			hasChanges = true;

			if (newType.variesByCulture) {
				// Invariant → Variant: keep existing culture values, only migrate invariant values if no value exists for that culture+segment
				for (const value of valuesOfAlias) {
					if (value.culture !== null) {
						// Keep existing culture values as-is
						result.push(value);
					} else {
						// Invariant value: only migrate if no value exists for default language + same segment
						const existingCultureValue = valuesOfAlias.find(
							(v) => v.culture === defaultLanguage && v.segment === value.segment,
						);
						if (!existingCultureValue) {
							result.push({ ...value, culture: defaultLanguage });
						}
					}
				}
			} else {
				// Variant → Invariant: keep existing invariant values, only migrate culture values if no invariant value exists for that segment
				const cultureToKeep = valuesOfAlias.some((v) => v.culture === defaultLanguage)
					? defaultLanguage
					: valuesOfAlias[0]?.culture;

				for (const value of valuesOfAlias) {
					if (value.culture === null) {
						// Keep existing invariant values as-is
						result.push(value);
					} else if (value.culture === cultureToKeep) {
						// Culture value: only migrate if no invariant value exists for the same segment
						const existingInvariantValue = valuesOfAlias.find(
							(v) => v.culture === null && v.segment === value.segment,
						);
						if (!existingInvariantValue) {
							result.push({ ...value, culture: null });
						}
					}
					// Discard values from other cultures
				}
			}
		}

		return { values: result, hasChanges };
	}
}
