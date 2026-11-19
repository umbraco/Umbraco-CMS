import type { UmbCollectionViewElementBase } from '../view/umb-collection-view-element-base.js';
import { UmbDefaultCollectionContext } from './collection-default.context.js';
import { UMB_COLLECTION_CONTEXT } from './collection-default.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Collection.Default',
	matchKind: 'default',
	matchType: 'collection',
	manifest: {
		type: 'collection',
		kind: 'default',
		elementName: 'umb-collection-default',
		api: UmbDefaultCollectionContext,
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-collection-default')
export class UmbCollectionDefaultElement extends UmbLitElement {
	#collectionContext?: UmbDefaultCollectionContext;

	@state()
	private _viewElement?: HTMLElement;

	@state()
	private _hasItems = false;

	@state()
	private _emptyLabel?: string;

	@state()
	private _initialLoadDone = false;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, async (context) => {
			this.#collectionContext = context;
			this.#observeIsLoading();
			this.#observeCurrentView();
			this.#observeTotalItems();
			this.#getEmptyStateLabel();
			this.#collectionContext?.loadCollection();
		});
	}

	#observeIsLoading() {
		if (!this.#collectionContext) return;
		let hasBeenLoading = false;

		this.observe(
			this.#collectionContext.loading,
			(isLoading) => {
				// We need to know when the initial loading has been done, to not show the empty state before that.
				// We can't just check if there are items, because there might be none.
				// So we check if it has been loading, and then when it stops loading we know the initial load is done.
				if (isLoading) {
					hasBeenLoading = true;
				} else if (hasBeenLoading) {
					this._initialLoadDone = true;
				}
			},
			'umbCollectionIsLoadingObserver',
		);
	}

	#observeCurrentView() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.view.currentView,
			async (manifest) => {
				if (!manifest) {
					this._viewElement = undefined;
					return;
				}

				const element = await createExtensionElement(manifest);

				// Views are imported on demand, so a slow import must not replace the view that became the current one
				// while it was in flight.
				if (manifest.alias !== this.#collectionContext?.view.getCurrentView()?.alias) return;

				if (element) {
					element.setAttribute('data-mark', `collection-view:${manifest.alias}`);
					(element as UmbCollectionViewElementBase).manifest = manifest;
				}
				this._viewElement = element;
			},
			'umbCollectionCurrentViewObserver',
		);
	}

	#observeTotalItems() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.totalItems,
			(totalItems) => {
				this._hasItems = totalItems > 0;
			},
			'umbCollectionTotalItemsObserver',
		);
	}

	#getEmptyStateLabel() {
		this._emptyLabel = this.#collectionContext?.getEmptyLabel();
	}

	override render() {
		return html`
			<umb-body-layout header-transparent class=${this._hasItems ? 'has-items' : ''}>
				${this.#renderBody()}
			</umb-body-layout>
		`;
	}

	#renderBody() {
		if (!this._initialLoadDone || !this._viewElement) return html`<umb-view-loader></umb-view-loader>`;

		return html`
			<div id="view">${this._viewElement}</div>
			${this.renderToolbar()} ${this._hasItems ? this.#renderContent() : this._renderEmptyState()}
		`;
	}

	protected renderToolbar() {
		return html`<umb-collection-toolbar slot="header"></umb-collection-toolbar>`;
	}

	protected renderPagination() {
		return html`<umb-collection-pagination></umb-collection-pagination>`;
	}

	protected renderSelectionActions() {
		return html`<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>`;
	}

	#renderContent() {
		return html`${this.renderPagination()} ${this.renderSelectionActions()}`;
	}

	protected _renderEmptyState() {
		return html`
			<div id="empty-state" class="uui-text">
				<h4>${this.localize.string(this._emptyLabel)}</h4>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}

			/* Sizes to the view it holds, so the empty state below it is not pushed out of sight when the view is empty. */
			#view {
				position: relative;
				visibility: hidden;
			}

			.has-items #view {
				visibility: visible;
			}

			#empty-state {
				height: 80%;
				align-content: center;
				text-align: center;
				opacity: 0;
				animation: fadeIn 200ms 200ms forwards;
			}

			@keyframes fadeIn {
				100% {
					opacity: 100%;
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-default': UmbCollectionDefaultElement;
	}
}
