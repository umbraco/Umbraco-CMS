import type { ManifestWorkspaceView } from '../../types.js';
import { UMB_WORKSPACE_VIEW_CONTEXT } from './workspace-view.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbHintController, type UmbVariantHint } from '@umbraco-cms/backoffice/hint';

/**
 *
 * NOTES
 * TODO:
 * The Workspace View Context is also a View Context...
 *
 * But shortcut should properly be it's own Context.
 * Consider leaving out Browser Title for now?
 * Could Hints be its own context? And can the navgiational element know about hints that the specific manager isnt available for — no need to spin up workspace view contexts virtually...
 *
 *
 * TODO: Enable changing Icon and Weight at runtime..
 *
 */
export class UmbWorkspaceViewContext extends UmbControllerBase {
	//
	#providerCtrl: any;
	#currentProvideHost?: UmbClassInterface;

	#variantId?: UmbVariantId;

	public manifest: ManifestWorkspaceView;

	public hints;

	constructor(host: UmbControllerHost, manifest: ManifestWorkspaceView, variantId?: UmbVariantId) {
		super(host);
		this.manifest = manifest;
		this.#variantId = variantId;
		this.hints = new UmbHintController<UmbVariantHint>(this, {
			viewAlias: manifest.alias,
			scaffold: {
				variantId: variantId,
			},
		});
	}

	provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_WORKSPACE_VIEW_CONTEXT, this);
		this.hints.provideAt(controllerHost);
	}

	unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
		this.hints.unprovide();
	}

	firstHintOfVariant(): Observable<UmbVariantHint | undefined> {
		if (this.#variantId) {
			return this.hints.asObservablePart((x) =>
				x.find((hint) => (hint.variantId ? hint.variantId.equal(this.#variantId!) : true)),
			);
		} else {
			return this.hints.firstHint;
		}
	}
}
