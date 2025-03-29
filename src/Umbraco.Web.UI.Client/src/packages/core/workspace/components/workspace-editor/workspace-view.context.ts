import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { ManifestWorkspaceView } from '../../types.js';
import { UMB_WORKSPACE_VIEW_CONTEXT } from './workspace-view.context-token.js';
import type { UUIInterfaceColor } from '@umbraco-ui/uui';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export type UmbWorkspaceViewNavigationState = {
	unique: string | symbol;
	text: string;
	weight: number;
	color?: UUIInterfaceColor;
};

export class UmbWorkspaceViewContext extends UmbControllerBase {
	//
	#providerCtrl: any;
	#currentProvideHost?: UmbClassInterface;

	manifest: ManifestWorkspaceView;

	#hints = new UmbArrayState<UmbWorkspaceViewNavigationState>([], (x) => x.unique);
	readonly hints = this.#hints.asObservable();
	readonly hint = this.#hints.asObservablePart((x) => x[0]);

	constructor(host: UmbControllerHost, manifest: ManifestWorkspaceView) {
		super(host);
		this.manifest = manifest;

		this.#hints.sortBy((a, b) => (b.weight || 0) - (a.weight || 0));
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

	hasHint(unique: string | symbol): boolean {
		return this.#hints.has(unique);
	}

	addHint(state: Partial<UmbWorkspaceViewNavigationState>): string | symbol {
		let newState = { ...state } as UmbWorkspaceViewNavigationState;
		newState.unique ??= Symbol();
		newState.weight ??= 0;
		newState.text ??= '!';
		this.#hints.appendOne(newState);
		return newState.unique;
	}

	removeHint(unique: string | symbol): void {
		this.#hints.removeOne(unique);
	}
}
