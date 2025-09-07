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
 */
export class UmbViewController extends UmbControllerBase {
	//
	#providerCtrl?: UmbContextProviderController;
	#consumeParentCtrl?: UmbContextConsumerController<typeof UMB_VIEW_CONTEXT.TYPE>;
	#currentProvideHost?: UmbClassInterface;
	#localize = new UmbLocalizationController(this);

	#active = false;
	#deactivatedFromOutside = false;
	#inherit?: boolean;
	#explicitInheritance?: boolean;
	#parentView?: UmbViewController;
	#title?: string;

	public readonly viewAlias: string | null;

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	protected readonly variantId = this.#variantId.asObservable();

	public hints;

	readonly firstHintOfVariant;

	constructor(host: UmbControllerHost, viewAlias: string | null) {
		super(host);
		this.viewAlias = viewAlias;
		this.hints = new UmbHintController<UmbVariantHint>(
			this,
			viewAlias
				? {
						viewAlias: viewAlias,
					}
				: undefined,
		);
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
			if (this.#active) {
				this.activate();
			}
		}).skipHost();
	}

	public setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
		this.hints.updateScaffold({ variantId: variantId });
	}

	public setBrowserTitle(title: string | undefined): void {
		this.#title = title;
		// TODO: This check should be if its the most child being active, but again think about how the parents in the active chain should work.
		if (this.#active) {
			this.#updateTitle();
		}
	}

	public provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_VIEW_CONTEXT, this);
		this.hints.provideAt(controllerHost);

		if (!this.#deactivatedFromOutside) {
			this.activate();
		}
	}

	public unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
		this.hints.unprovide();

		// TODO: should be call deactivate?
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

	public inherit() {
		this.#inherit = true;
		this.#updateTitle();
	}

	public inheritFrom(context?: UmbViewController): void {
		this.#explicitInheritance = true;
		this.#consumeParentCtrl?.destroy();
		this.#consumeParentCtrl = undefined;
		this.#parentView = context;
		this.#inheritFromParent();
	}

	#inheritFromParent(): void {
		if (this.#parentView && this.#active) {
			this.#parentView.deactivate();
		}
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
		if (this.#parentView) {
			// Missing option to deactivate the parent not part of this inheritance chain.
			this.#parentView.deactivate();
		}
		this.#updateTitle();
	}

	#updateTitle() {
		const localTitle = this.getComputedTitle();
		document.title = (localTitle ? localTitle + ' | ' : '') + 'Umbraco';
	}

	public getComputedTitle(): string | undefined {
		const titles = [];
		if (this.#parentView) {
			titles.push(this.#parentView.getComputedTitle());
		}
		if (this.#title) {
			titles.push(this.#localize.string(this.#title));
		}
		return titles.length > 0 ? titles.join(' | ') : undefined;
	}

	/**
	 * Deactivate the view context.
	 * We cannot conclude that this means the parent should be activated, it can be because of a child being activated.
	 */
	public deactivate() {
		if (!this.#active) return;
		this.#deactivatedFromOutside = true;
	}
}
