import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { ManifestCollectionView } from '../extensions/types.js';
import type { UmbCollectionLayoutConfiguration } from '../types.js';
import { css, customElement, html, nothing, query, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { UMB_ROUTE_CONTEXT } from '@umbraco-cms/backoffice/router';

interface UmbCollectionViewLayout {
	alias: string;
	label: string;
	icon: string;
	pathName: string;
}

@customElement('umb-collection-view-bundle')
export class UmbCollectionViewBundleElement extends UmbLitElement {
	@state()
	_views: Array<UmbCollectionViewLayout> = [];

	@state()
	_currentView?: UmbCollectionViewLayout;

	@state()
	private _collectionRootPathName?: string;

	@state()
	private _entityUnique?: string;

	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ROUTE_CONTEXT, (context) => {
			this.observe(context?.activePath, (activePath) => {
				this._collectionRootPathName = activePath;
			});
		});

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;
			this.#observeCollection();
		});

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (context) => {
			this._entityUnique = context?.getUnique() ?? '';
		});
	}

	#observeCollection() {
		if (!this.#collectionContext) return;

		this.observe(
			this.#collectionContext.view.currentView,
			(currentView) => {
				if (!currentView) return;
				this._currentView = this._views.find((view) => view.alias === currentView.alias);
			},
			'umbCurrentCollectionViewObserver',
		);

		this.observe(
			observeMultiple([this.#collectionContext.view.views, this.#collectionContext.viewLayouts]),
			([manifests, viewLayouts]) => {
				if (!manifests?.length && !viewLayouts?.length) return;
				this._views = this.#mapManifestToViewLayout(manifests, viewLayouts);
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
					alias: viewManifest.alias,
					label: viewLayout.name ?? viewManifest.meta.label,
					icon: viewLayout.icon ?? viewManifest.meta.icon,
					pathName: viewManifest.meta.pathName,
				});
			});

			return layouts;
		}

		// fallback on the 'collectionView' manifests
		return manifests.map((manifest) => ({
			alias: manifest.alias,
			label: manifest.meta.label,
			icon: manifest.meta.icon,
			pathName: manifest.meta.pathName,
		}));
	}

	#onClick(view: UmbCollectionViewLayout) {
		this.#collectionContext?.setLastSelectedView(this._entityUnique, view.alias);

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
							(view) => view.alias,
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
				href="${this._collectionRootPathName}/${view.pathName}"
				@click-label=${() => this.#onClick(view)}
				?active=${view.alias === this._currentView?.alias}>
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
