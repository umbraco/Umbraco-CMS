import type { ManifestWorkspaceView } from '../../types.js';
import type { UmbWorkspaceHint } from '../../controllers/workspace-view-hint-manager.controller.js';
import { UMB_WORKSPACE_VIEW_CONTEXT } from './workspace-view.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbHintManager } from '@umbraco-cms/backoffice/utils';

export interface UmbWorkspaceViewHint extends UmbWorkspaceHint {
	viewAlias: string;
}

export class UmbWorkspaceViewContext extends UmbControllerBase {
	//
	#providerCtrl: any;
	#currentProvideHost?: UmbClassInterface;

	public manifest: ManifestWorkspaceView;

	public hints;

	constructor(host: UmbControllerHost, manifest: ManifestWorkspaceView) {
		super(host);
		this.manifest = manifest;
		this.hints = new UmbHintManager<UmbWorkspaceViewHint>(this, { scaffold: { viewAlias: manifest.alias } });
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

	/**
	 * observe hint of an optional variant, the variant is optional, in that case all variantIds are accepted.
	 * @param {UmbVariantId} variantId - the variantId to match against the hint.
	 * @returns {Observable<UmbWorkspaceHint | undefined>} the first hint that matches the variantId or undefined if no hint is found.
	 */
	hintOfVariant(variantId?: UmbVariantId): Observable<UmbWorkspaceHint | undefined> {
		const viewAlias = this.manifest.alias;
		if (variantId) {
			return this.hints.asObservablePart((x) =>
				x.find((hint) => hint.viewAlias === viewAlias && (hint.variantId ? hint.variantId.compare(variantId) : true)),
			);
		}
		return this.hints.asObservablePart((x) => x.find((hint) => hint.viewAlias === viewAlias));
	}
}
