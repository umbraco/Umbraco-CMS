import { UmbShortcutController } from '../../shortcut/context/shortcut.controller.js';
import { UMB_VIEW_CONTEXT } from './view.context-token.js';
import { _setUmbCurrentViewTitle } from './current-view-title.js';
import type { UmbCurrentViewTitleSegment, UmbViewTitleKind } from './current-view-title.js';
import { UmbClassState, UmbObjectState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbHintController } from '@umbraco-cms/backoffice/hint';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { UmbContextConsumerController, UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 *
 * The View Context handles the aspects of three Features:
 * Browser Titles — Provide a title for this view and it will be set or joint with parent views depending on the inheritance setting.
 * Hints — Holds Hints for this view, depending on the inheritance setting it will propagate the hints to be displayed at parent views.
 * Shortcuts — Not implemented yet.
 *
 */
export class UmbViewController extends UmbControllerBase {
	//
	static #ActiveView?: UmbViewController;
	//
	#attached = false;
	#providerCtrl?: UmbContextProviderController;
	#consumeParentCtrl?: UmbContextConsumerController<typeof UMB_VIEW_CONTEXT.TYPE>;
	#currentProvideHost?: UmbClassInterface;
	#localize = new UmbLocalizationController(this);

	// State used to know if the context can be auto activated when attached.
	#autoActivate = true;
	#active = false;
	get isActive() {
		return this.#active;
	}
	#setActive() {
		this.#active = true;
		if (this.#inherit) {
			// Secure the parent in the inheritance chain is active.
			this.#parentView?._internal_activate();
		} else {
			// This is for a single, or top level of the inheritance chain, so we can disable the previous active view.
			if (UmbViewController.#ActiveView && UmbViewController.#ActiveView !== this) {
				UmbViewController.#ActiveView._internal_deactivate();
				UmbViewController.#ActiveView = undefined;
			}
			UmbViewController.#ActiveView = this;
		}
	}
	#removeActive() {
		this.#active = false;
		if (!this.#inherit) {
			if (UmbViewController.#ActiveView === this) {
				UmbViewController.#ActiveView = undefined;
			}
		}
	}

	#inherit = false;
	#explicitInheritance?: boolean;
	#parentView?: UmbViewController;
	#title?: string;
	#titleKind: UmbViewTitleKind = 'workspace';
	#titleTypeLabel?: string;
	#titleAncestors?: ReadonlyArray<string>;
	#titleIcon?: string;
	#computedTitleSegments = new UmbObjectState<ReadonlyArray<UmbCurrentViewTitleSegment> | undefined>(undefined);
	readonly computedTitleSegments = this.#computedTitleSegments.asObservable();
	readonly computedTitle = this.#computedTitleSegments.asObservablePart((segs) =>
		segs?.length ? segs.map((s) => s.label).join(' | ') : undefined,
	);

	public readonly viewAlias: string | null;

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	protected readonly variantId = this.#variantId.asObservable();

	public readonly hints;

	public readonly shortcuts = new UmbShortcutController(this);

	public readonly firstHintOfVariant;

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
			if (parentView) {
				this.#setParentView(parentView);
			}
			// only activate if we had an incoming parentView, cause if not we are in a disassembling state. [NL]
			if (parentView && this.#attached && this.#autoActivate) {
				this._internal_requestActivate();
			}
		}).skipHost();
	}

	#setParentView(view: UmbViewController | undefined) {
		if (this.#parentView === view) return;
		this.#parentView = view;

		if (this.#inherit) {
			this.#inheritFromParent();
		}
	}

	public setVariantId(variantId: UmbVariantId | undefined): void {
		this.#variantId.setValue(variantId);
		this.hints.updateScaffold({ variantId: variantId });
	}

	/**
	 * Set this view's title. When the view is active (and has no active children),
	 * the title contributes to `document.title` and to the published breadcrumb.
	 * @param title The view's own title. Localization keys like `#foo_bar` are resolved automatically.
	 * @param options Optional metadata. `kind` classifies the segment (default: `'workspace'`). `typeLabel`
	 *   optionally inserts an additional segment with kind `'workspace-type'` immediately before this one — useful
	 *   for entity workspaces that want to disambiguate e.g. "User Group" from "User" in the breadcrumb. `icon`
	 *   attaches an icon to the leaf segment (e.g. `icon-document-js`) so consumers like the user history list
	 *   can render an entity-specific icon alongside the label.
	 */
	public setTitle(
		title: string | undefined,
		options?: { kind?: UmbViewTitleKind; typeLabel?: string; icon?: string },
	): void {
		const { kind = 'workspace', typeLabel, icon } = options ?? {};
		if (
			this.#title === title &&
			this.#titleKind === kind &&
			this.#titleTypeLabel === typeLabel &&
			this.#titleIcon === icon
		) {
			return;
		}
		this.#title = title;
		this.#titleKind = kind;
		this.#titleTypeLabel = typeLabel;
		this.#titleIcon = icon;
		this.#computeTitle();
		this.#updateTitle();
	}

	/**
	 * Set additional breadcrumb segments representing ancestors of this view in a
	 * hierarchical entity tree (e.g. the parent folders of a media item). Ancestors
	 * are inserted into the title chain between the parent view's contribution and
	 * this view's own title, with `kind: 'workspace-ancestor'`.
	 *
	 * Ordered root → leaf (closest ancestor last). Pass `undefined` or an empty
	 * array to clear.
	 * @param ancestors Ancestor labels, root-most first.
	 */
	public setAncestors(ancestors: ReadonlyArray<string> | undefined): void {
		const normalized = ancestors?.length ? ancestors : undefined;
		const current = this.#titleAncestors;
		if (current === normalized) return;
		if (current && normalized && current.length === normalized.length && current.every((v, i) => v === normalized[i]))
			return;
		this.#titleAncestors = normalized;
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
		this.shortcuts.provideAt(controllerHost);

		if (this.#attached) {
			this._internal_requestActivate();
		}
	}

	public unprovide(): void {
		if (this.#providerCtrl) {
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
		}
		this.hints.unprovide();
		this.shortcuts.unprovide();

		this._internal_deactivate();
		this.#requestActivateParent();
	}

	override hostConnected(): void {
		const wasActive = this.isActive;
		const wasAttached = this.#attached;
		this.#attached = true;
		super.hostConnected();
		if (!wasAttached) {
			this.#parentView?._internal_addChild(this);
		}
		// Check that we have a providerController, otherwise this is not provided. [NL]
		if (this.#autoActivate && !wasActive) {
			this._internal_requestActivate();
		}
	}

	override hostDisconnected(): void {
		const wasAttached = this.#attached;
		this.#attached = false;
		if (wasAttached) {
			this.#parentView?._internal_removeChild(this);
		}

		this._internal_deactivate();
		super.hostDisconnected();
		this.#autoActivate = true;
		this.#requestActivateParent();
	}

	public isInheriting() {
		return this.#inherit;
	}

	public inherit() {
		this.#inherit = true;
	}

	public inheritFrom(context?: UmbViewController): void {
		this.#inherit = true;
		this.#explicitInheritance = true;
		this.#consumeParentCtrl?.destroy();
		this.#consumeParentCtrl = undefined;
		this.#setParentView(context);
	}

	#inheritFromParent(): void {
		this.observe(
			this.#parentView?.variantId,
			(variantId) => {
				this.setVariantId(variantId);
			},
			'observeParentVariantId',
		);
		// Observe the full segment list rather than the joined string so that
		// metadata-only changes (icon, kind) on parent segments propagate to
		// inheriting children even when the labels haven't changed.
		this.observe(
			this.#parentView?.computedTitleSegments,
			() => {
				this.#computeTitle();
				// Check for parent view as it is undefined in a disassembling state and we do not want to update the title in that situation. [NL]
				if (this.#providerCtrl && this.#parentView && this.isActive) {
					this.#updateTitle();
				}
			},
			'observeParentTitle',
		);
		// Hint inheritance requires a viewAlias or pathFilter on this view to target.
		// Views without an alias (e.g. workspace-level views inheriting from a section
		// purely to receive the section's title chain) have no hint target to resolve
		// and must skip hint inheritance — title inheritance alone is supported.
		if (this.viewAlias) {
			this.hints.inheritFrom(this.#parentView?.hints);
		}
	}

	#requestActivateParent() {
		if (!this.#inherit) {
			if (this.#parentView) {
				this.#parentView._internal_requestActivate();
			}
		}
	}

	/**
	 * @internal
	 * Notify that a view context has been activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_requestActivate(): boolean {
		if (!this.#providerCtrl) {
			// If we are not provided we should not be activated. [NL]
			return false;
		}
		// TODO: Check this one: We do not want a parent to auto activate if a child is having the activation. [NL], well maybe it not that bad because of the asking of the children...
		this.#autoActivate = true;
		if (this.isActive) {
			return true;
		}
		// If not attached then propagate the activation to the parent. [NL]
		if (this.#attached === false) {
			if (!this.#parentView) {
				throw new Error('Cannot activate a view that is not attached to the DOM.');
			}
		} else {
			// Check if any of the children likes to be activated instead:
			// A reverse loop ensures latest added child gets first chance to activate. This may matter in some future issue-scenario, I will say it could be that it is not the right way to determine if multiple children wants to be active. [NL]
			let i = this.#children.length;
			while (i--) {
				const child = this.#children[i];
				if (child._internal_requestActivate()) {
					// If we have an active child we should not update the title.
					return true;
				}
			}
			// if not then check your self:
			if (this.#autoActivate && this.#attached) {
				this._internal_activate();
				return true;
			}
		}
		return false;
	}

	/**
	 * @internal
	 * Notify that a view context has been activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_activate() {
		if (this.#attached) {
			this.#autoActivate = true;
			this.#setActive();
			this.#updateTitle();
			this.shortcuts.activate();
		}
	}

	/**
	 * @internal
	 * Deactivate the view context.
	 * We cannot conclude that this means the parent should be activated, it can be because of a child being activated.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_deactivate() {
		if (!this.isActive) return;
		this.#autoActivate = false;

		// Deactive children:
		this.#children.forEach((child) => {
			if (child.isInheriting()) {
				child._internal_deactivate();
			}
		});
		this.shortcuts.deactivate();
		this.#removeActive();
	}

	#updateTitle() {
		if (!this.#active || this.#hasActiveChildren()) {
			return;
		}
		const segments = this.#computedTitleSegments.getValue();
		if (!segments || segments.length === 0) {
			return;
		}
		const localTitle = segments.map((s) => s.label).join(' | ');
		document.title = localTitle + ' | Umbraco';

		_setUmbCurrentViewTitle({
			path: window.location.pathname,
			segments,
		});
	}

	#computeTitle() {
		const segments: UmbCurrentViewTitleSegment[] = [];
		if (this.#inherit && this.#parentView) {
			const parentSegments = this.#parentView.getComputedTitleSegments();
			if (parentSegments) {
				segments.push(...parentSegments);
			}
		}
		if (this.#title) {
			if (this.#titleTypeLabel) {
				segments.push({
					label: this.#localize.string(this.#titleTypeLabel),
					kind: 'workspace-type',
				});
			}
			if (this.#titleAncestors) {
				for (const ancestor of this.#titleAncestors) {
					// Ancestor names can arrive as localization keys (e.g. a tree root's
					// `#treeHeaders_dataTypes`), so resolve them the same way the leaf and
					// type-label titles do.
					segments.push({ label: this.#localize.string(ancestor), kind: 'workspace-ancestor' });
				}
			}
			segments.push({
				label: this.#localize.string(this.#title),
				kind: this.#titleKind,
				...(this.#titleIcon ? { icon: this.#titleIcon } : {}),
			});
		}
		// Collapse consecutive segments with the same label. Primary case: a tree
		// root sharing its hosting section's name (e.g. "Content" root under the
		// Content section) — dedup avoids "Content | Content". Side effect: if a
		// child entity has the exact same name as its parent (e.g. folder "Images"
		// inside folder "Images"), the breadcrumb loses one level; the full URL
		// is preserved on the history `path` so navigation still works.
		const deduped: UmbCurrentViewTitleSegment[] = [];
		for (const seg of segments) {
			if (deduped[deduped.length - 1]?.label === seg.label) continue;
			deduped.push(seg);
		}
		this.#computedTitleSegments.setValue(deduped.length ? deduped : undefined);
	}

	public getComputedTitle(): string | undefined {
		const segs = this.#computedTitleSegments.getValue();
		return segs?.length ? segs.map((s) => s.label).join(' | ') : undefined;
	}

	public getComputedTitleSegments(): ReadonlyArray<UmbCurrentViewTitleSegment> | undefined {
		return this.#computedTitleSegments.getValue();
	}

	#children: UmbViewController[] = [];
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_addChild(child: UmbViewController) {
		this.#children.push(child);
		if (this.isActive) {
			child._internal_activate();
		}
	}
	// eslint-disable-next-line @typescript-eslint/naming-convention
	public _internal_removeChild(child: UmbViewController) {
		const index = this.#children.indexOf(child);
		if (index !== -1) {
			this.#children.splice(index, 1);
		}
		// update title?
		if (!this.#hasActiveChildren()) {
			if (this.#active) {
				this.#updateTitle();
			} else if (this.#providerCtrl && this.#attached) {
				// If we're not active but should be (no active children, attached, and provided),
				// reactivate to restore the title
				this._internal_activate();
			}
		}
	}
	#hasActiveChildren() {
		return this.#children.some((child) => child.isActive);
	}

	override destroy(): void {
		this.#inherit = false;
		this.#removeActive();
		this.#autoActivate = false;
		(this as any).provideAt = undefined;
		this.unprovide();
		super.destroy();
		this.#consumeParentCtrl = undefined;
	}
}
