import { UMB_COLLECTION_CONTEXT, UmbCollectionContext } from './collection.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, property } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { ManifestCollectionView } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-collection')
export class UmbCollectionElement extends UmbLitElement {
	@state()
	private _routes: Array<UmbRoute> = [];

	private _entityType!: string;
	@property({ type: String, attribute: 'entity-type' })
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		this._entityType = value;
	}

	protected collectionContext?: UmbCollectionContext<any, any>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.collectionContext = context;
			this.#observeCollectionViews();
		});
	}

	#observeCollectionViews() {
		this.observe(this.collectionContext!.views, (views) => {
			this._createRoutes(views);
		}),
			'umbCollectionViewsObserver';
	}

	private _createRoutes(views: ManifestCollectionView[] | null) {
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
	}

	render() {
		return html`
			<umb-body-layout header-transparent>
				${this.renderToolbar()}
				<umb-router-slot id="router-slot" .routes="${this._routes}"></umb-router-slot>
				${this.renderPagination()}
				${this.renderSelectionActions()}
			</umb-body-layout>
		`;
	}

	protected renderToolbar() {
		return html`<umb-collection-toolbar slot="header"></umb-collection-toolbar>`;
	}

	protected renderPagination () {
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
