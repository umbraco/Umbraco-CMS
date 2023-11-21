import { UmbCollectionContext } from './collection.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, property } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { type ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-collection')
export class UmbCollectionElement extends UmbLitElement {
	@property({ type: String, reflect: true })
	get alias() {
		return this.#collectionContext.getAlias();
	}
	set alias(newVal) {
		this.#collectionContext.setAlias(newVal);
	}

	@state()
	private _routes: Array<UmbRoute> = [];

	#collectionContext = new UmbCollectionContext(this);

	constructor() {
		super();
		this.#observeCollectionViews();
	}

	#observeCollectionViews() {
		this.observe(this.#collectionContext.views, (views) => {
			this.#createRoutes(views);
		}),
			'umbCollectionViewsObserver';
	}

	#createRoutes(views: ManifestCollectionView[] | null) {
		this._routes = [];

		if (views) {
			this._routes = views.map((view) => {
				return {
					path: `${view.meta.pathName}`,
					component: () => createExtensionElement(view),
				};
			});
		}

		this._routes.push({
			path: '',
			redirectTo: views?.[0]?.meta.pathName ?? '/',
		});

		this.requestUpdate();
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
		'umb-collection': UmbCollectionElement;
	}
}
