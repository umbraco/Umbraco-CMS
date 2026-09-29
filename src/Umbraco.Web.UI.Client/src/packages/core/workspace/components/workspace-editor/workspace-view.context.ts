import type { ManifestWorkspaceView } from '../../types.js';
import { UmbWorkspaceViewController } from './workspace-view.controller.js';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

export class UmbWorkspaceViewContext extends UmbWorkspaceViewController {
	constructor(host: UmbClassInterface, manifest: ManifestWorkspaceView) {
		super(host, manifest);
		this.provideAt(host);
	}
}
