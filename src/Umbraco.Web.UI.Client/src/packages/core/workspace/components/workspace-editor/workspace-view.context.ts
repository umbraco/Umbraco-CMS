import type { ManifestWorkspaceView } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbViewContext } from '@umbraco-cms/backoffice/view';
export class UmbWorkspaceViewContext extends UmbViewContext {
	public readonly IS_WORKSPACE_VIEW_CONTEXT = true as const;

	// Note: manifest can change later, but because we currently only use the alias from it, it's not something we need to handle. [NL]
	public manifest: ManifestWorkspaceView;

	constructor(host: UmbControllerHost, manifest: ManifestWorkspaceView) {
		super(host, manifest.alias);
		this.manifest = manifest;
	}
}
