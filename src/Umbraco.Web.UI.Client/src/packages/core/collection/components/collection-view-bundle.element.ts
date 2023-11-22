import { UMB_COLLECTION_CONTEXT, UmbCollectionDefaultContext } from '../collection-default.context.js';
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
	private _collectionRootPathname = '';

	#collectionContext?: UmbCollectionDefaultContext<any, any>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			if (!this.#collectionContext) return;
			this._collectionRootPathname = this.#collectionContext.collectionRootPathname;
			this.#observeViews();
			this.#observeCurrentView();
		});
	}

	#observeCurrentView() {
		this.observe(
			this.#collectionContext!.currentView,
			(view) => {
				//TODO: This is not called when the view is changed
				this._currentView = view;
			},
			'umbCurrentCollectionViewObserver',
		);
	}

	#observeViews() {
		this.observe(
			this.#collectionContext!.views,
			(views) => {
				this._views = views;
			},
			'umbCollectionViewsObserver',
		);
	}
	render() {
		if (!this._currentView) return nothing;
		if (this._views.length <= 1) return nothing;

		return html`
			<uui-button compact popovertarget="collection-view-bundle-popover" label="status">
				${this.#renderItemDisplay(this._currentView)}
			</uui-button>
			<uui-popover-container id="collection-view-bundle-popover" popover placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">${this._views.map((view) => this.#renderItem(view))}</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderItem(view: ManifestCollectionView) {
		return html`
			<uui-button compact href="${this._collectionRootPathname}/${view.meta.pathName}">
				${this.#renderItemDisplay(view)} <span class="label">${view.meta.label}</span>
			</uui-button>
		`;
	}

	#renderItemDisplay(view: ManifestCollectionView) {
		return html`<uui-icon name=${view.meta.icon}></uui-icon>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				--uui-button-content-align: left;
			}
			.label {
				margin-left: var(--uui-size-space-1);
			}
			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-bundle': UmbCollectionViewBundleElement;
	}
}
