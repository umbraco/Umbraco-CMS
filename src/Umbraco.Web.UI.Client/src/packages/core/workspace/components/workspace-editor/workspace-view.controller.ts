import type { ManifestWorkspaceView } from '../../types.js';
import { UmbViewController } from '@umbraco-cms/backoffice/view';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

export class UmbWorkspaceViewController extends UmbViewController {
	public readonly IS_WORKSPACE_VIEW_CONTEXT = true as const;

	// Note: manifest can change later, but because we currently only use the alias from it, it's not something we need to handle. [NL]
	public manifest: ManifestWorkspaceView;

	constructor(host: UmbClassInterface, manifest: ManifestWorkspaceView) {
		super(host, manifest.alias);
		this.manifest = manifest;
	}
}
