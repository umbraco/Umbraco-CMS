import type { UmbContentDetailWorkspaceContextBase } from './content-detail-workspace-base.js';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * @class UmbContentDetailWorkspaceTypeTransformController
 * @description - Controller to handle content detail workspace type transformations, such as property variation changes.
 */
export class UmbContentDetailWorkspaceTypeTransformController extends UmbControllerBase {
	#workspace: UmbContentDetailWorkspaceContextBase<any, any, any, any>;
	// Current property types, currently used to detect variation changes:
	#propertyTypes?: Array<UmbPropertyTypeModel>;

	constructor(host: UmbContentDetailWorkspaceContextBase<any, any, any, any>) {
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

		// Get default language:
		// get the next value of this observable:
		const languages = this.#workspace.getLanguages();
		const defaultLanguage = languages.find((lang) => lang.isDefault)?.unique;
		if (!defaultLanguage) {
			throw new Error('Default language not found');
		}

		const values = currentData.values.map((v) => {
			const oldType = oldPropertyTypes.find((p) => p.alias === v.alias);
			const newType = newPropertyTypes.find((p) => p.alias === v.alias);
			if (!oldType || !newType) {
				// If we cant find both, we do not dare changing anything. Notice a composition may not have been loaded yet.
				return v;
			}
			if (oldType.variesByCulture !== newType.variesByCulture) {
				// Variation has changed, we need to migrate this value
				if (newType.variesByCulture) {
					// If it now varies by culture, set to default language:
					return { ...v, culture: defaultLanguage };
				} else {
					// If it no longer varies by culture, set to invariant:
					return { ...v, culture: null };
				}
			}
			return v;
		});

		this.#workspace.setData({ ...currentData, values });
	}
}
