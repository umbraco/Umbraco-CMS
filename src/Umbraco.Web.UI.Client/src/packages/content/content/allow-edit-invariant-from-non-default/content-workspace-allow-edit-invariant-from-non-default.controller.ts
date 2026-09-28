import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../workspace/content-workspace.context-token.js';
import { UmbContentAllowEditInvariantFromNonDefaultControllerBase } from './content-allow-edit-invariant-from-non-default-controller-base.js';

/**
 * Makes invariant (shared) properties of a content workspace read-only when the current user lacks
 * the invariant-for-variant permission.
 */
export class UmbContentWorkspaceAllowEditInvariantFromNonDefaultController extends UmbContentAllowEditInvariantFromNonDefaultControllerBase {
	protected async _preventEditInvariantFromNonDefault() {
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

export { UmbContentWorkspaceAllowEditInvariantFromNonDefaultController as api };
