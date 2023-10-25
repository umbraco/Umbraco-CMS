import { UMB_COLLECTION_CONTEXT, UmbCollectionContext } from '../collection.context.js';
import { ManifestCollectionView } from '../../extension-registry/models/collection-view.model.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-collection-view-bundle')
export class UmbCollectionViewBundleElement extends UmbLitElement {
	@state()
	_views: Array<ManifestCollectionView> = [];

	@state()
	_currentView?: ManifestCollectionView;

	@state()
	private _isOpen = false;

	#collectionContext?: UmbCollectionContext<any, any>;
	#collectionRootPath = '';

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			this.#observeViews();
			this.#observeCurrentView();
		});
	}

	#observeCurrentView() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext?.currentView, (view) => {
			this._currentView = view;
		});
	}

	#observeViews() {
		if (!this.#collectionContext) return;

		this.observe(this.#collectionContext?.views, (views) => {
			this._views = views;
		});
	}

	#toggleDropdown() {
		this._isOpen = !this._isOpen;
	}

	#closeDropdown() {
		this._isOpen = false;
	}

	render() {
		return html`${this.#renderLayoutButton()}`;
	}

	#renderLayoutButton() {
		if (!this._currentView) return nothing;

		return html` <umb-dropdown .open="${this._isOpen}" @close=${this.#closeDropdown}>
			<uui-button slot="trigger" label="status" @click=${this.#toggleDropdown}
				>${this.#renderItemDisplay(this._currentView)}</uui-button
			>
			<div slot="dropdown" class="filter-dropdown">${this._views.map((view) => this.#renderItem(view))}</div>
		</umb-dropdown>`;
	}

	#renderItem(view: ManifestCollectionView) {		
		return html`<a href="${view.meta.pathName}">${this.#renderItemDisplay(view)}</a>`;
	}

	#renderItemDisplay(view: ManifestCollectionView) {
		return html`<span class="item"><uui-icon name=${view.meta.icon}></uui-icon> ${view.meta.label}</span>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			.item {
			}

			a { 
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-bundle': UmbCollectionViewBundleElement;
	}
}
