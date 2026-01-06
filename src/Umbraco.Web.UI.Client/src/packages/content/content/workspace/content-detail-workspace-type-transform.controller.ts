import type { UmbContentDetailModel } from '../index.js';
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

		// To spare a bit of energy we keep awareness of whether this brings any changes:
		let hasChanges = false;
		const values = currentData.values.map((v) => {
			const transformation = this.#transformValueForVariationChange(
				v,
				oldPropertyTypes,
				newPropertyTypes,
				defaultLanguage,
			);
			if (transformation.changed) {
				hasChanges = true;
			}
			return transformation.value;
		});

		if (hasChanges) {
			this.#workspace.setData({ ...currentData, values });
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

	#transformValueForVariationChange(
		value: any,
		oldPropertyTypes: Array<UmbPropertyTypeModel>,
		newPropertyTypes: Array<UmbPropertyTypeModel>,
		defaultLanguage: string,
	): { value: any; changed: boolean } {
		const oldType = oldPropertyTypes.find((p) => p.alias === value.alias);
		const newType = newPropertyTypes.find((p) => p.alias === value.alias);

		// If we cant find both, we do not dare changing anything. Notice a composition may not have been loaded yet.
		if (!oldType || !newType) {
			return { value, changed: false };
		}

		// If variation hasn't changed, return unchanged
		if (oldType.variesByCulture === newType.variesByCulture) {
			return { value, changed: false };
		}

		// Variation has changed, migrate the value
		if (newType.variesByCulture) {
			// If it now varies by culture, set to default language
			return { value: { ...value, culture: defaultLanguage }, changed: true };
		} else {
			// If it no longer varies by culture, set to invariant
			return { value: { ...value, culture: null }, changed: true };
		}
	}
}
