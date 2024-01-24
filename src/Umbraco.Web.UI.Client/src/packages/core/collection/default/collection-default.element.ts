import { UMB_DEFAULT_COLLECTION_CONTEXT, UmbDefaultCollectionContext } from './collection-default.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

const manifest: UmbBackofficeManifestKind = {
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

	#collectionContext?: UmbDefaultCollectionContext<any, any>;

	constructor() {
		super();
		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionRoutes();
		});
	}

	#observeCollectionRoutes() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext.view.routes, (routes) => {
			this._routes = routes;
		}),
			'umbCollectionRoutesObserver';
	}

	render() {
		return html`
			<umb-body-layout header-transparent>
				${this.renderToolbar()}
				<umb-router-slot id="router-slot" .routes="${this._routes}"></umb-router-slot>
				${this.renderPagination()} ${this.renderSelectionActions()}
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
		return html`<umb-collection-selection-actions slot="footer-info"></umb-collection-selection-actions>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
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
