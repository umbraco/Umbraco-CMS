import { UmbShortcutController } from '../../shortcut/context/shortcut.controller.js';
import { UMB_VIEW_CONTEXT } from './view.context-token.js';
import { _setUmbCurrentViewTitle } from './current-view-title.js';
import type { UmbCurrentViewTitleSegment, UmbViewTitleKind } from './current-view-title.js';
import { UmbClassState, UmbObjectState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbHintController } from '@umbraco-cms/backoffice/hint';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
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
	#segmentSlots = new Map<string, ReadonlyArray<UmbCurrentViewTitleSegment>>();
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
	 * Add or replace segments under a named slot. Each slot key is owned by a
	 * single caller (e.g. `'leaf'`, `'workspace-type'`, `'ancestors'`).
	 * Labels may be raw localization keys (`#foo_bar`) — they are resolved
	 * automatically when the title is computed.
	 * @param slotKey Unique key identifying this caller's contribution.
	 * @param segments One or more segments to store under the slot. Pass none to clear.
	 */
	public setSegments(slotKey: string, ...segments: UmbCurrentViewTitleSegment[]): void {
		if (segments.length === 0) {
			this.clearSegments(slotKey);
			return;
		}
		if (UmbViewController.#segmentsEqual(this.#segmentSlots.get(slotKey), segments)) return;
		this.#segmentSlots.set(slotKey, segments);
		this.#computeTitle();
		this.#updateTitle();
	}

	/**
	 * Remove all segments for a named slot.
	 * @param slotKey The slot to clear.
	 */
	public clearSegments(slotKey: string): void {
		if (!this.#segmentSlots.has(slotKey)) return;
		this.#segmentSlots.delete(slotKey);
		this.#computeTitle();
		this.#updateTitle();
	}

	/**
	 * @deprecated Use {@link setSegments} instead. Scheduled for removal in Umbraco 19.
	 * Convenience wrapper that maps the legacy positional API to named segment slots.
	 */
	public setTitle(
		title: string | undefined,
		options?: { kind?: UmbViewTitleKind; typeLabel?: string; icon?: string },
	): void {
		new UmbDeprecation({
			deprecated: 'UmbViewController.setTitle()',
			removeInVersion: '19.0.0',
			solution: 'Use view.setSegments() and view.clearSegments() instead.',
		}).warn();
		const { kind = 'workspace', typeLabel, icon } = options ?? {};
		// Batch slot mutations to avoid intermediate publishes.
		if (title) {
			if (typeLabel) {
				this.#segmentSlots.set('workspace-type', [{ label: typeLabel, kind: 'workspace-type' }]);
			} else {
				this.#segmentSlots.delete('workspace-type');
			}
			this.#segmentSlots.set('leaf', [{ label: title, kind, ...(icon ? { icon } : {}) }]);
		} else {
			this.#segmentSlots.delete('workspace-type');
			this.#segmentSlots.delete('leaf');
		}
		this.#computeTitle();
		this.#updateTitle();
	}

	/**
	 * @deprecated Use {@link setSegments} with kind `'workspace-ancestor'` instead. Scheduled for removal in Umbraco 19.
	 * Convenience wrapper that maps ancestor labels to the `'ancestors'` segment slot.
	 */
	public setAncestors(ancestors: ReadonlyArray<string> | undefined): void {
		new UmbDeprecation({
			deprecated: 'UmbViewController.setAncestors()',
			removeInVersion: '19.0.0',
			solution: 'Use view.setSegments() with kind "workspace-ancestor" instead.',
		}).warn();
		if (!ancestors?.length) {
			if (!this.#segmentSlots.has('ancestors')) return;
			this.#segmentSlots.delete('ancestors');
		} else {
			this.#segmentSlots.set(
				'ancestors',
				ancestors.map((label): UmbCurrentViewTitleSegment => ({ label, kind: 'workspace-ancestor' })),
			);
		}
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
		// If the parent was already resolved (synchronous context lookup) before
		// inherit() was called, #setParentView saw #inherit=false and skipped
		// #inheritFromParent. Re-evaluate now that inheritance is enabled.
		if (this.#parentView) {
			this.#inheritFromParent();
		}
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
		// Views without either (e.g. workspace-level views inheriting from a section
		// purely to receive the section's title chain) have no hint target to resolve
		// and must skip hint inheritance — title inheritance alone is supported.
		if (this.viewAlias || this.hints.hasPathFilter) {
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

		// Clear the global title when the top-level (non-inheriting) active view
		// deactivates, so subscribers don't see stale data between navigations.
		if (!this.#inherit) {
			_setUmbCurrentViewTitle(undefined);
		}
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

	static #KIND_PRIORITY: Record<UmbViewTitleKind, number> = {
		section: 0,
		'workspace-type': 1,
		'workspace-ancestor': 2,
		workspace: 3,
		tab: 4,
		modal: 5,
	};

	static #segmentsEqual(
		a: ReadonlyArray<UmbCurrentViewTitleSegment> | undefined,
		b: ReadonlyArray<UmbCurrentViewTitleSegment> | undefined,
	): boolean {
		if (a === b) return true;
		if (!a || !b || a.length !== b.length) return false;
		return a.every((s, i) =>
			s.label === b[i].label && s.kind === b[i].kind && s.icon === b[i].icon && s.replaces === b[i].replaces,
		);
	}

	#computeTitle() {
		const segments: UmbCurrentViewTitleSegment[] = [];

		// 1. Inherited parent segments (already localized by the parent's own computation).
		if (this.#inherit && this.#parentView) {
			const parentSegments = this.#parentView.getComputedTitleSegments();
			if (parentSegments) {
				segments.push(...parentSegments);
			}
		}

		// 2. This view's own segments: flatten all slots, sort by kind priority,
		//    and localize labels (which may be raw localization keys like `#foo_bar`).
		const local: UmbCurrentViewTitleSegment[] = [];
		for (const slot of this.#segmentSlots.values()) {
			for (const seg of slot) {
				local.push({
					...seg,
					label: this.#localize.string(seg.label),
				});
			}
		}
		local.sort(
			(a, b) => (UmbViewController.#KIND_PRIORITY[a.kind] ?? 99) - (UmbViewController.#KIND_PRIORITY[b.kind] ?? 99),
		);
		segments.push(...local);

		// 3. Apply explicit replacements: when a segment has `replaces: true` and
		//    the preceding segment has the same label, replace it. This is opt-in
		//    so callers control exactly when dedup happens (no hidden metadata loss).
		const resolved: UmbCurrentViewTitleSegment[] = [];
		for (const seg of segments) {
			const prev = resolved[resolved.length - 1];
			if (seg.replaces && prev?.label === seg.label) {
				resolved[resolved.length - 1] = seg;
				continue;
			}
			resolved.push(seg);
		}
		const result = resolved.length ? resolved : undefined;
		if (UmbViewController.#segmentsEqual(this.#computedTitleSegments.getValue(), result)) return;
		this.#computedTitleSegments.setValue(result);
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
