import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../constants.js';
import { UmbDocumentAllowEditInvariantFromNonDefaultControllerBase } from './document-allow-edit-invariant-from-non-default-controller-base.js';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentWorkspaceAllowEditInvariantFromNonDefaultController extends UmbDocumentAllowEditInvariantFromNonDefaultControllerBase {
	protected async _preventEditInvariantFromNonDefault() {
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
					const rule: UmbVariantPropertyGuardRule = this._createRule({ datasetVariantId });

					documentWorkspaceContext.propertyWriteGuard.addRule(rule);
				});
			},
		);
	}
}

export { UmbDocumentWorkspaceAllowEditInvariantFromNonDefaultController as api };
