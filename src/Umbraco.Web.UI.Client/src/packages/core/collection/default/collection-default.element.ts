import { UmbDefaultCollectionContext } from './collection-default.context.js';
import { UMB_COLLECTION_CONTEXT } from './collection-default.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

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
	@state()
	private _routes: Array<UmbRoute> = [];

	@state()
	private _hasItems = false;

	#collectionContext?: UmbDefaultCollectionContext<any, any>;

	constructor() {
		super();
		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			this.#collectionContext?.requestCollection();
			this.#observeCollectionRoutes();
			this.#observeTotalItems();
		});
	}

	#observeCollectionRoutes() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.view.routes,
			(routes) => {
				this._routes = routes;
			},
			'umbCollectionRoutesObserver',
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

	override render() {
		return html`
			<umb-body-layout header-transparent>
				${this.renderToolbar()} ${this._hasItems ? this.#renderContent() : this.#renderEmptyState()}
			</umb-body-layout>
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
		return html`
			<umb-router-slot .routes=${this._routes}></umb-router-slot>
			${this.renderPagination()} ${this.renderSelectionActions()}
		`;
	}

	#renderEmptyState() {
		return html` <div id="empty-state" class="uui-text">
			<h4><umb-localize key="collection_noItemsTitle"></umb-localize></h4>
		</div>`;
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

			#empty-state {
				height: 100%;
				align-content: center;
				text-align: center;
			}

			router-slot {
				width: 100%;
				height: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-default': UmbCollectionDefaultElement;
	}
}
