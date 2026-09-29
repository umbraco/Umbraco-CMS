import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../workspace/content-workspace.context-token.js';
import { UmbContentInvariantForVariantGuardControllerBase } from './content-invariant-for-variant-guard-controller-base.js';

/**
 * Makes invariant (shared) properties of a content workspace read-only when the current user lacks
 * the invariant-for-variant permission.
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
	}
}

export { UmbContentWorkspaceInvariantForVariantGuardController as api };
