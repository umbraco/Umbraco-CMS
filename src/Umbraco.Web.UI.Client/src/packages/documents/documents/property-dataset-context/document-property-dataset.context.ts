import type { UmbDocumentDetailModel, UmbDocumentVariantModel, UmbDocumentWorkspaceContext } from '../types.js';
import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../global-contexts/index.js';
import { UmbContentPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { DocumentConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';

export class UmbDocumentPropertyDatasetContext extends UmbContentPropertyDatasetContext<
	UmbDocumentDetailModel,
	UmbDocumentTypeDetailModel,
	UmbDocumentVariantModel
> {
	#dataSetVariantId?: UmbVariantId;
	#documentConfiguration?: DocumentConfigurationResponseModel;
	#allowEditInvariantFromNonDefault: boolean | undefined = undefined;

	constructor(host: UmbControllerHost, dataOwner: UmbDocumentWorkspaceContext, variantId?: UmbVariantId) {
		super(host, dataOwner, variantId);

		this.#dataSetVariantId = variantId;

		this.consumeContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT, async (context) => {
			this.#documentConfiguration = (await context?.getDocumentConfiguration()) ?? undefined;
			this.#allowEditInvariantFromNonDefault = this.#documentConfiguration?.allowEditInvariantFromNonDefault;
			this.#enforceAllowEditInvariantFromNonDefault();
		});
	}

	#enforceAllowEditInvariantFromNonDefault() {
		this.observe(
			observeMultiple([this._dataOwner.structure.contentTypeProperties, this._dataOwner.variantOptions]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				const currentVariantOption = variantOptions.find(
					(option) => option.culture === this.#dataSetVariantId?.culture,
				);

				const isDefaultLanguage = currentVariantOption?.language.isDefault;

				properties.forEach((property) => {
					const unique = 'UMB_PREVENT_EDIT_INVARIANT_FROM_NON_DEFAULT_' + property.unique;

					// Clean up the rule if it exists before adding a new one
					this._dataOwner.propertyWriteGuard.removeRule(unique);

					// If the property is invariant and not in the default language, we need to add a rule
					if (!this.#allowEditInvariantFromNonDefault && !property.variesByCulture && !isDefaultLanguage) {
						const rule: UmbVariantPropertyGuardRule = {
							unique,
							message: 'Shared properties can only be edited in the default language',
							propertyType: {
								unique: property.unique,
							},
							permitted: false,
						};

						this._dataOwner.propertyWriteGuard.addRule(rule);
					}
				});
			},
		);
	}
}
