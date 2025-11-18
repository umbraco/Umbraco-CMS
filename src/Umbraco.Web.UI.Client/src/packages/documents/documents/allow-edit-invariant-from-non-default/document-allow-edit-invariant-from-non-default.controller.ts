import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../global-contexts/index.js';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/block';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

export class UmbDocumentAllowEditInvariantFromNonDefaultController extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT, async (context) => {
			const config = await context?.getDocumentConfiguration();
			const allowEditInvariantFromNonDefault = config?.allowEditInvariantFromNonDefault ?? true;

			if (allowEditInvariantFromNonDefault === false) {
				this.#preventEditInvariantFromNonDefault();
			}
		});
	}

	async #preventEditInvariantFromNonDefault() {
		const documentWorkspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		});

		if (!documentWorkspaceContext) {
			throw new Error('Missing Document Workspace Context');
		}

		this.observe(
			observeMultiple([
				documentWorkspaceContext.structure.contentTypeProperties,
				documentWorkspaceContext.variantOptions,
			]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				variantOptions.forEach((variantOption) => {
					// Do not add a rule for the default language. It is always permitted to edit.
					if (variantOption.language.isDefault) return;

					const datasetVariantId = UmbVariantId.CreateFromPartial(variantOption);
					const invariantVariantId = UmbVariantId.CreateInvariant();
					const unique = `UMB_PREVENT_EDIT_INVARIANT_FROM_NON_DEFAULT_DATASET=${datasetVariantId.toString()}_PROPERTY_${invariantVariantId.toString()}`;

					const rule: UmbVariantPropertyGuardRule = {
						unique,
						message: 'Shared properties can only be edited in the default language',
						variantId: invariantVariantId,
						datasetVariantId,
						permitted: false,
					};

					documentWorkspaceContext.propertyWriteGuard.addRule(rule);
				});
			},
		);
	}
}

export { UmbDocumentAllowEditInvariantFromNonDefaultController as api };
