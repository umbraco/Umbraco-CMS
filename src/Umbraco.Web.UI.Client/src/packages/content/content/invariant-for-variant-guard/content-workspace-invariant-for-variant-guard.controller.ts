import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../workspace/content-workspace.context-token.js';
import { UmbContentInvariantForVariantGuardControllerBase } from './content-invariant-for-variant-guard-controller-base.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

const PREVENT_WRITE_INVARIANT_RULE_UNIQUE = 'UMB_PREVENT_WRITE_INVARIANT_FOR_VARIANT';

/**
 * Makes invariant (shared) properties of a content workspace read-only, and its invariant variant non-writable,
 * when the current user lacks the invariant-for-variant permission.
 */
export class UmbContentWorkspaceInvariantForVariantGuardController extends UmbContentInvariantForVariantGuardControllerBase {
	protected async _preventEditInvariantForVariant() {
		const contentWorkspaceContext = await this.getContext(UMB_CONTENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		});

		if (!contentWorkspaceContext) {
			throw new Error('Missing Content Workspace Context');
		}

		this._observeAndApplyRule({
			propertiesObservable: contentWorkspaceContext.structure.contentTypeProperties,
			variantOptionsObservable: contentWorkspaceContext.variantOptions,
			variesByCultureObservable: contentWorkspaceContext.structure.variesByCulture,
			propertyWriteGuard: contentWorkspaceContext.propertyWriteGuard,
		});

		this.observe(
			contentWorkspaceContext.structure.variesByCulture,
			(variesByCulture) => {
				if (variesByCulture) {
					contentWorkspaceContext.variantWriteGuard.addRule({
						unique: PREVENT_WRITE_INVARIANT_RULE_UNIQUE,
						variantId: UmbVariantId.CreateInvariant(),
						permitted: false,
						message: 'You do not have permission to edit shared (invariant) properties on this content.',
					});
				} else {
					contentWorkspaceContext.variantWriteGuard.removeRule(PREVENT_WRITE_INVARIANT_RULE_UNIQUE);
				}
			},
			'_observeVariesByCultureForInvariantVariantWrite',
		);
	}
}

export { UmbContentWorkspaceInvariantForVariantGuardController as api };
