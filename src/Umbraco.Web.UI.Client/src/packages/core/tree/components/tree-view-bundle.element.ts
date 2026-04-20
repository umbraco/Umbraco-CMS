import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import type { ManifestTreeView } from '../view/types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-tree-view-bundle')
export class UmbTreeViewBundleElement extends UmbLitElement {
	@state()
	private _views: Array<ManifestTreeView> = [];

	@state()
	private _currentView?: ManifestTreeView;

	#treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.#treeContext = context;
			this.#observeViews();
		});
	}

	#observeViews() {
		if (!this.#treeContext?.view) return;

		this.observe(
			this.#treeContext.view.views,
			(views) => {
				this._views = views;
			},
			'umbTreeViewsObserver',
		);

		this.observe(
			this.#treeContext.view.currentView,
			(currentView) => {
				this._currentView = currentView;
			},
			'umbTreeCurrentViewObserver',
		);
	}

	@query('#tree-view-bundle-popover')
	private _popover?: UUIPopoverContainerElement;

	#onClick(view: ManifestTreeView) {
		this.#treeContext?.view?.setCurrentView(view);

		setTimeout(() => {
			// TODO: This ignorer is just needed for JSON SCHEMA TO WORK, As its not updated with latest TS yet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._popover?.hidePopover();
		}, 100);
	}

	override render() {
		if (!this._currentView) return nothing;
		if (this._views.length <= 1) return nothing;

		return html`
			<uui-button compact popovertarget="tree-view-bundle-popover" label=${this.localize.term('general_switchView')}>
				<umb-icon name=${this._currentView.meta.icon}></umb-icon>
			</uui-button>
			<uui-popover-container id="tree-view-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="view-dropdown">
						${repeat(
							this._views,
							(view) => view.alias,
							(view) => this.#renderItem(view),
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderItem(view: ManifestTreeView) {
		return html`
			<uui-menu-item
				label=${view.meta.label}
				@click-label=${() => this.#onClick(view)}
				?active=${view.alias === this._currentView?.alias}>
				<umb-icon slot="icon" name=${view.meta.icon}></umb-icon>
			</uui-menu-item>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				--uui-button-content-align: left;
				--uui-menu-item-flat-structure: 1;
				display: contents;
			}

			.view-dropdown {
				padding: var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-view-bundle': UmbTreeViewBundleElement;
	}
}
