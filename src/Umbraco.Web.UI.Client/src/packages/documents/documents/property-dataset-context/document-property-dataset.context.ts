import type { UmbDocumentDetailModel, UmbDocumentVariantModel, UmbDocumentWorkspaceContext } from '../types.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, type UmbVariantPropertyReadOnlyState } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbDocumentDetailModel,
	UmbDocumentTypeDetailModel,
	UmbDocumentVariantModel
> {
	constructor(host: UmbControllerHost, dataOwner: UmbDocumentWorkspaceContext, variantId?: UmbVariantId) {
		super(host, dataOwner, variantId);

		this.observe(
			observeMultiple([this._dataOwner.structure.contentTypeProperties, this._dataOwner.variantOptions]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				const currentVariantOption = variantOptions.find((option) => option.culture === variantId?.culture);
				const isDefaultLanguage = currentVariantOption?.language.isDefault;

				properties.forEach((property) => {
					const unique = 'UMB_PREVENT_SHARED_PROPERTY_EDITING_FROM_NON_DEFAULT_' + property.unique;

					this._dataOwner.structure.propertyReadOnlyState.removeState(unique);

					if (!property.variesByCulture && !isDefaultLanguage) {
						const state: UmbVariantPropertyReadOnlyState = {
							unique,
							message: 'Shared properties can only be edited in the default language',
							propertyType: {
								unique: property.unique,
								variantId: new UmbVariantId(),
							},
						};

						this._dataOwner.structure.propertyReadOnlyState.addState(state);
					}
				});
			},
		);
	}
}
