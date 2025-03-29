import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { ManifestWorkspaceView } from '../../types.js';
import { UMB_WORKSPACE_VIEW_CONTEXT } from './workspace-view.context-token.js';

export class UmbWorkspaceViewContext extends UmbControllerBase {
	//
	#providerCtrl: any;
	#currentProvideHost?: UmbClassInterface;

	manifest: ManifestWorkspaceView;

	constructor(host: UmbControllerHost, manifest: ManifestWorkspaceView) {
		super(host);
		this.manifest = manifest;
	}

	provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_WORKSPACE_VIEW_CONTEXT, this);
	}

	unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
	}
}
