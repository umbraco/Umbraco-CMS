import type { ManifestWorkspaceView } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbViewContext } from '@umbraco-cms/backoffice/view';
export class UmbWorkspaceViewContext extends UmbViewContext {
	public readonly IS_WORKSPACE_VIEW_CONTEXT = true as const;

	public manifest: ManifestWorkspaceView;

	constructor(host: UmbControllerHost, manifest: ManifestWorkspaceView, variantId?: UmbVariantId) {
		super(host, manifest.alias, variantId);
		this.manifest = manifest;
	}
}
