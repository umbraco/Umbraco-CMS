import { UMB_VIEW_CONTEXT } from './view.context-token.js';
import {
	UmbBooleanState,
	UmbClassState,
	UmbStringState,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbHintController } from '@umbraco-cms/backoffice/hint';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

const ObserveParentActiveCtrlAlias = Symbol();

/**
 *
 * TODO:
 * Include Shortcuts
 *
 * The View Context handles the aspects of three Features:
 * Browser Titles — Provide a title for this view and it will be set or joint with parent views depending on the inheritance setting.
 * Hints — Holds Hints for this view, depending on the inheritance setting it will propagate the hints to be displayed at parent views.
 * Shortcuts — Not implemented yet.
 *
 */
export class UmbViewController extends UmbControllerBase {
	//
	#attached = false;
	#providerCtrl?: UmbContextProviderController;
	#consumeParentCtrl?: UmbContextConsumerController<typeof UMB_VIEW_CONTEXT.TYPE>;
	#currentProvideHost?: UmbClassInterface;
	#localize = new UmbLocalizationController(this);

	// State used to know if the context can be auto activated when attached.
	#autoActivate = true;
	#active = new UmbBooleanState(false);
	public readonly active = this.#active.asObservable();
	get isActive() {
		return this.#active.getValue();
	}
	#hasActiveChild = false;
	#inherit?: boolean;
	#explicitInheritance?: boolean;
	#parentView?: UmbViewController;
	#title?: string;
	#computedTitle = new UmbStringState(undefined);
	readonly computedTitle = this.#computedTitle.asObservable();

	public readonly viewAlias: string | null;

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	protected readonly variantId = this.#variantId.asObservable();

	public hints;

	readonly firstHintOfVariant;

	constructor(host: UmbControllerHost, viewAlias: string | null) {
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
			if (this.isActive && !this.#hasActiveChild) {
				// If we were active we will react as if we got deactivated and then activated again below if state allows. [NL]
				this.#propagateActivation();
			}
			this.#active.setValue(false);
			if (parentView) {
				this.#parentView = parentView;
			}
			if (this.#inherit) {
				this.#inheritFromParent();
			}
			// only activate if we had an incoming parentView, cause if not we are in a disassembling state. [NL]
			if (parentView && this.#attached && this.#autoActivate) {
				this._internal_activate();
			}
		}).skipHost();
	}

	public setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
		this.hints.updateScaffold({ variantId: variantId });
	}

	public setBrowserTitle(title: string | undefined): void {
		if (this.#title === title) return;
		this.#title = title;
		// TODO: This check should be if its the most child being active, but again think about how the parents in the active chain should work.
		this.#computeTitle();
		this.#updateTitle();
	}

	public provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#autoActivate = true;
		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_VIEW_CONTEXT, this);
		this.hints.provideAt(controllerHost);

		if (this.#attached && this.#autoActivate) {
			this._internal_activate();
		}
	}

	public unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
		this.hints.unprovide();

		this._internal_deactivate();
	}

	override hostConnected(): void {
		const wasActive = this.isActive;
		this.#attached = true;
		super.hostConnected();
		// Check that we have a providerController, otherwise this is not provided. [NL]
		if (this.#autoActivate && !wasActive) {
			this._internal_activate();
		}
	}

	override hostDisconnected(): void {
		const wasAttached = this.#attached;
		const wasActive = this.isActive;
		this.#attached = false;
		this.#active.setValue(false);
		super.hostDisconnected();
		if (wasAttached === true && wasActive) {
			// Check that we have a providerController, otherwise this is not provided. [NL]
			this.#propagateActivation();
		}
	}

	public inherit() {
		this.#inherit = true;
	}

	public inheritFrom(context?: UmbViewController): void {
		this.#inherit = true;
		this.#explicitInheritance = true;
		this.#consumeParentCtrl?.destroy();
		this.#consumeParentCtrl = undefined;
		this.#parentView = context;
		// Notice because we cannot break the inheritance, we do not need to stop this observation in any of the logic. [NL]
		this.observe(
			this.#parentView?.active,
			(isActive) => {
				if (isActive) {
					this._internal_activate();
				} else {
					this._internal_deactivate();
				}
			},
			ObserveParentActiveCtrlAlias,
		);
		this.#inheritFromParent();
		this.#propagateActivation();
	}

	#inheritFromParent(): void {
		this.observe(
			this.#parentView?.variantId,
			(variantId) => {
				this.setVariantId(variantId);
			},
			'observeParentVariantId',
		);
		this.observe(
			this.#parentView?.computedTitle,
			() => {
				this.#computeTitle();
				// Check for parent view as it is undefined in a disassembling state and we do not want to update the title in that situation. [NL]
				if (this.#providerCtrl && this.#parentView && this.isActive) {
					this.#updateTitle();
				}
			},
			'observeParentTitle',
		);
		this.hints.inheritFrom(this.#parentView?.hints);
	}

	#propagateActivation() {
		if (!this.#parentView) return;
		if (this.#inherit) {
			if (this.isActive) {
				this.#parentView._internal_childActivated();
			} else {
				this.#parentView._internal_childDeactivated();
			}
		} else {
			if (this.isActive) {
				this.#parentView._internal_deactivate();
			} else {
				this.#parentView._internal_activate();
			}
		}
	}

	/**
	 * @internal
	 * Notify that a view context has been activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_activate() {
		if (!this.#providerCtrl) {
			// If we are not provided we should not be activated. [NL]
			return;
		}
		this.#autoActivate = true;
		if (this.isActive) {
			return;
		}
		// If not attached then propagate the activation to the parent. [NL]
		if (this.#attached === false) {
			if (!this.#parentView) {
				throw new Error('Cannot activate a view that is not attached to the DOM.');
			}
			this.#propagateActivation();
		} else {
			this.#active.setValue(true);
			this.#propagateActivation();
			this.#updateTitle();
			// TODO: Start shortcuts. [NL]
		}
	}

	/**
	 * @internal
	 * Notify that a child has been activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_childActivated() {
		if (this.#hasActiveChild) return;
		this.#hasActiveChild = true;
		this._internal_activate();
	}

	/**
	 * @internal
	 * Notify that a child is no longer activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_childDeactivated() {
		this.#hasActiveChild = false;
		if (this.#attached === false) {
			if (this.#parentView) {
				return;
			} else {
				throw new Error('Cannot re-activate(_childDeactivated) a view that is not attached to the DOM.');
			}
		}
		if (this.#autoActivate) {
			this._internal_activate();
		} else {
			this.#propagateActivation();
		}
	}

	/**
	 * @internal
	 * Deactivate the view context.
	 * We cannot conclude that this means the parent should be activated, it can be because of a child being activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_deactivate() {
		this.#autoActivate = false;
		if (!this.isActive) return;
		this.#active.setValue(false);
		// TODO: Stop shortcuts. [NL]
		// Deactivate parents:
		this.#propagateActivation();
	}

	#updateTitle() {
		if (!this.#active || this.#hasActiveChild) {
			return;
		}
		const localTitle = this.getComputedTitle();
		document.title = (localTitle ? localTitle + ' | ' : '') + 'Umbraco';
	}

	#computeTitle() {
		const titles = [];
		if (this.#inherit && this.#parentView) {
			titles.push(this.#parentView.getComputedTitle());
		}
		if (this.#title) {
			titles.push(this.#localize.string(this.#title));
		}
		this.#computedTitle.setValue(titles.length > 0 ? titles.join(' | ') : undefined);
	}

	public getComputedTitle(): string | undefined {
		return this.#computedTitle.getValue();
	}

	override destroy(): void {
		this.#inherit = false;
		this.#active.setValue(false);
		this.#autoActivate = false;
		(this as any).provideAt = undefined;
		this.unprovide();
		super.destroy();
		this.#consumeParentCtrl = undefined;
	}
}
