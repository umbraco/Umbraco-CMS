import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import { UmbClassState, mergeObservables, type Observable } from '@umbraco-cms/backoffice/observable-api';
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

	public readonly viewAlias: string;
	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	protected readonly _variantId = this.#variantId.asObservable();

	public hints;

	readonly firstHintOfVariant;

	constructor(host: UmbControllerHost, viewAlias: string, variantId?: UmbVariantId) {
		super(host);
		this.viewAlias = viewAlias;
		this.#variantId.setValue(variantId);
		this.hints = new UmbHintController<UmbVariantHint>(this, {
			viewAlias: viewAlias,
			scaffold: {
				variantId: variantId,
			},
		});
		this.firstHintOfVariant = mergeObservables([this._variantId, this.hints.hints], ([variantId, hints]) => {
			if (variantId) {
				return hints.find((hint) => (hint.variantId ? hint.variantId.equal(variantId!) : true));
			} else {
				return hints[0];
			}
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

	inheritFrom(context?: UmbViewContext): void {
		// TODO: Do you want to inherit the variantId as well? Then I think VariantId needs to become a state.
		this.hints.inheritFrom(context?.hints);
	}
}
