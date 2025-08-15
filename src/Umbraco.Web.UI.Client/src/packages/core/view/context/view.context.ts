import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbHintController, type UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { UMB_VIEW_CONTEXT } from './view.context-token';

/**
 *
 * TODO:
 * Include Shortcuts
 *
 * Browser Title?
 *
 */
export class UmbViewContext extends UmbControllerBase {
	//
	#providerCtrl: any;
	#currentProvideHost?: UmbClassInterface;

	#variantId?: UmbVariantId;

	public hints;

	constructor(host: UmbControllerHost, viewAlias: string, variantId?: UmbVariantId) {
		super(host);
		this.#variantId = variantId;
		this.hints = new UmbHintController<UmbVariantHint>(this, {
			viewAlias: viewAlias,
			scaffold: {
				variantId: variantId,
			},
		});
	}

	provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_VIEW_CONTEXT, this);
		this.hints.provideAt(controllerHost);
	}

	unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
		this.hints.unprovide();
	}

	inheritFrom(context: UmbViewContext): void {
		// TODO: Do you want to inherit the variantId as well? Then I think VariantId needs to become a state.
		this.hints.inheritFrom(context.hints);
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
