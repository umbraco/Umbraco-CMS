import { UMB_VIEW_CONTEXT } from './view.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import { UmbClassState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbHintController, type UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';

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
	#providerCtrl?: UmbContextProviderController;
	#consumeParentCtrl?: UmbContextConsumerController<typeof UMB_VIEW_CONTEXT.TYPE>;
	#currentProvideHost?: UmbClassInterface;
	#localize = new UmbLocalizationController(this);

	#active = false;
	#deactivatedFromOutside = false;
	#explicitInheritance?: boolean;
	#parentView?: UmbViewContext;
	#title?: string;

	public readonly viewAlias: string;

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	protected readonly variantId = this.#variantId.asObservable();

	public hints;

	readonly firstHintOfVariant;

	constructor(host: UmbControllerHost, viewAlias: string) {
		super(host);
		this.viewAlias = viewAlias;
		this.hints = new UmbHintController<UmbVariantHint>(this, {
			viewAlias: viewAlias,
		});
		this.firstHintOfVariant = mergeObservables([this.variantId, this.hints.hints], ([variantId, hints]) => {
			// Notice, because we in UI have invariant fields on Variants, then we will accept invariant hints on variants.
			if (variantId) {
				return hints.find((hint) =>
					hint.variantId ? hint.variantId.equal(variantId!) || hint.variantId.isInvariant() : true,
				);
			} else {
				return hints[0];
			}
		});

		this.#consumeParentCtrl = this.consumeContext(UMB_VIEW_CONTEXT, (parentView) => {
			// In case of explicit inheritance we do not want to overview the parent view.
			if (this.#explicitInheritance) return;
			this.#parentView = parentView;
		}).skipHost();
	}

	public setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
		this.hints.updateScaffold({ variantId: variantId });
	}

	public setBrowserTitle(title: string): void {
		this.#title = title;
	}

	public provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_VIEW_CONTEXT, this);
		this.hints.provideAt(controllerHost);
	}

	public unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
		this.hints.unprovide();
	}

	override hostConnected(): void {
		super.hostConnected();
		// CHeck that we have a providerController, otherwise this is not provided. [NL]
		if (this.#providerCtrl && !this.#deactivatedFromOutside) {
			this.activate();
		}
	}

	override hostDisconnected(): void {
		// CHeck that we have a providerController, otherwise this is not provided. [NL]
		if (this.#providerCtrl && this.#parentView) {
			this.#parentView.activate();
		}
		super.hostDisconnected();
	}

	public inheritFrom(context?: UmbViewContext): void {
		this.#explicitInheritance = true;
		this.#consumeParentCtrl?.destroy();
		this.#consumeParentCtrl = undefined;
		this.#parentView = context;
		this.#inheritFromParent();
	}

	#inheritFromParent(): void {
		this.observe(
			this.#parentView?.variantId,
			(variantId) => {
				this.setVariantId(variantId);
			},
			'observeParentVariantId',
		);
		this.hints.inheritFrom(this.#parentView?.hints);
	}

	public activate() {
		this.#active = true;
		if (this.#title) {
			document.title = this.#localize.string(this.#title);
		}
	}

	/**
	 * Deactivate the view context.
	 * We cannot conclude that this means the parent should be activated, it can be because of a child being activated.
	 */
	public deactivate() {
		this.#deactivatedFromOutside = true;
	}
}
