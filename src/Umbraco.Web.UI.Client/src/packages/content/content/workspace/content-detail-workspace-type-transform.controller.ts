import type { UmbContentDetailModel, UmbContentValueModel } from '../index.js';
import type { UmbContentDetailWorkspaceContextBase } from './content-detail-workspace-base.js';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

interface UmbTypeTransformPersistedDataAccessor<DetailModelType> {
	getPersisted(): DetailModelType | undefined;
	setPersisted(data: DetailModelType | undefined): void;
}

/**
 * @class UmbContentDetailWorkspaceTypeTransformController
 * @description - Controller to handle content detail workspace type transformations, such as property variation changes.
 */
export class UmbContentDetailWorkspaceTypeTransformController<
	DetailModelType extends UmbContentDetailModel<UmbEntityVariantModel>,
> extends UmbControllerBase {
	#workspace: UmbContentDetailWorkspaceContextBase<DetailModelType, any, any, any>;
	#persistedData?: UmbTypeTransformPersistedDataAccessor<DetailModelType>;
	// Current property types, currently used to detect variation changes:
	#propertyTypes?: Array<UmbPropertyTypeModel>;
	#variesByCulture?: boolean;

	constructor(
		host: UmbContentDetailWorkspaceContextBase<DetailModelType, any, any, any>,
		persistedData?: UmbTypeTransformPersistedDataAccessor<DetailModelType>,
	) {
		super(host);

		this.#workspace = host;
		this.#persistedData = persistedData;

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

		// Observe the owner content type's culture variance to migrate the variants collection alongside
		// the values, so the active variant keeps its name, state and dates and the next save matches the type
		this.observe(
			host.structure.ownerContentTypeObservablePart((x) => x?.variesByCulture),
			(variesByCulture: boolean | undefined) => {
				const previousVariesByCulture = this.#variesByCulture;
				this.#variesByCulture = variesByCulture;
				if (
					previousVariesByCulture === undefined ||
					variesByCulture === undefined ||
					previousVariesByCulture === variesByCulture
				) {
					return;
				}
				this.#handleContentTypeVarianceChange(variesByCulture);
			},
			null,
		);
	}

	#handleContentTypeVarianceChange(variesByCulture: boolean): void {
		const currentData = this.#workspace.getData();
		if (!currentData) return;

		const defaultLanguage = this.#getDefaultLanguage();
		const transformVariants = (variants: DetailModelType['variants']) =>
			variesByCulture
				? this.#transformVariantsToCultureVariant(variants, defaultLanguage)
				: this.#transformVariantsToInvariant(variants, defaultLanguage);

		this.#workspace.setData({ ...currentData, variants: transformVariants(currentData.variants) });

		// The persisted data must be migrated too — the server migrated its copy when the content type
		// was saved, and constructing save data merges the persisted variants back in:
		const persisted = this.#persistedData?.getPersisted();
		if (persisted) {
			this.#persistedData?.setPersisted({ ...persisted, variants: transformVariants(persisted.variants) });
		}
	}

	/**
	 * Moves invariant variant entries to the default language, keeping existing culture entries as-is.
	 */
	#transformVariantsToCultureVariant(
		variants: DetailModelType['variants'],
		defaultLanguage: string,
	): DetailModelType['variants'] {
		const result: DetailModelType['variants'] = [];
		for (const variant of variants) {
			if (variant.culture !== null) {
				result.push(variant);
				continue;
			}
			const hasCultureVariant = variants.some((v) => v.culture === defaultLanguage && v.segment === variant.segment);
			if (!hasCultureVariant) {
				result.push({ ...variant, culture: defaultLanguage });
			}
		}
		return result;
	}

	/**
	 * Collapses culture variant entries to invariant, preferring the default language and discarding other cultures.
	 */
	#transformVariantsToInvariant(
		variants: DetailModelType['variants'],
		defaultLanguage: string,
	): DetailModelType['variants'] {
		const cultureToKeep = variants.some((v) => v.culture === defaultLanguage)
			? defaultLanguage
			: variants.find((v) => v.culture !== null)?.culture;

		const result: DetailModelType['variants'] = [];
		for (const variant of variants) {
			if (variant.culture === null) {
				result.push(variant);
			} else if (variant.culture === cultureToKeep) {
				const hasInvariantVariant = variants.some((v) => v.culture === null && v.segment === variant.segment);
				if (!hasInvariantVariant) {
					result.push({ ...variant, culture: null });
				}
			}
		}
		return result;
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
						const existingInvariantValue = valuesOfAlias.find((v) => v.culture === null && v.segment === value.segment);
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
