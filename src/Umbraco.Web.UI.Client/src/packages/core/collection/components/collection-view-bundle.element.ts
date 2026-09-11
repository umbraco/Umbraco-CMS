import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { ManifestCollectionView } from '../view/types.js';
import type { UmbCollectionLayoutConfiguration } from '../types.js';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

interface UmbCollectionViewLayout {
	manifest: ManifestCollectionView;
	label: string;
	icon: string;
}

@customElement('umb-collection-view-bundle')
export class UmbCollectionViewBundleElement extends UmbLitElement {
	@state()
	private _views: Array<UmbCollectionViewLayout> = [];

	@state()
	private _currentView?: UmbCollectionViewLayout;

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			this.#observeCollection();
		});
	}

	#observeCollection() {
		if (!this.#collectionContext) return;

		// The available views and the current one are resolved together, as the current view is presented through the
		// layout of one of the available views.
		this.observe(
			observeMultiple([
				this.#collectionContext.view.views,
				this.#collectionContext.viewLayouts,
				this.#collectionContext.view.currentView,
			]),
			([manifests, viewLayouts, currentView]) => {
				if (!manifests?.length && !viewLayouts?.length) return;
				this._views = this.#mapManifestToViewLayout(manifests, viewLayouts);
				this._currentView = this._views.find((view) => view.manifest.alias === currentView?.alias);
			},
			'umbCollectionViewsAndLayoutsObserver',
		);
	}

	@query('#collection-view-bundle-popover')
	private _popover?: UUIPopoverContainerElement;

	#mapManifestToViewLayout(
		manifests: Array<ManifestCollectionView>,
		viewLayouts: Array<UmbCollectionLayoutConfiguration>,
	): typeof this._views {
		if (viewLayouts.length > 0) {
			const layouts: typeof this._views = [];

			viewLayouts.forEach((viewLayout) => {
				const viewManifest = manifests.find((manifest) => manifest.alias === viewLayout.collectionView);
				if (!viewManifest) return;
				layouts.push({
					manifest: viewManifest,
					label: viewLayout.name ?? viewManifest.meta.label,
					icon: viewLayout.icon ?? viewManifest.meta.icon,
				});
			});

			return layouts;
		}

		// fallback on the 'collectionView' manifests
		return manifests.map((manifest) => ({
			manifest,
			label: manifest.meta.label,
			icon: manifest.meta.icon,
		}));
	}

	#onClick(view: UmbCollectionViewLayout) {
		this.#collectionContext?.view.setCurrentView(view.manifest);

		setTimeout(() => {
			// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._popover?.hidePopover();
		}, 100);
	}

	override render() {
		if (!this._currentView) return nothing;
		if (this._views.length <= 1) return nothing;

		return html`
			<uui-button compact popovertarget="collection-view-bundle-popover" label="status">
				<umb-icon name=${this._currentView.icon}></umb-icon>
			</uui-button>
			<uui-popover-container id="collection-view-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${repeat(
							this._views,
							(view) => view.manifest.alias,
							(view) => this.#renderItem(view),
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderItem(view: UmbCollectionViewLayout) {
		return html`
			<uui-menu-item
				label=${view.label}
				@click-label=${() => this.#onClick(view)}
				?active=${view.manifest.alias === this._currentView?.manifest.alias}>
				<umb-icon slot="icon" name=${view.icon}></umb-icon>
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

			.filter-dropdown {
				padding: var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-bundle': UmbCollectionViewBundleElement;
	}
}
