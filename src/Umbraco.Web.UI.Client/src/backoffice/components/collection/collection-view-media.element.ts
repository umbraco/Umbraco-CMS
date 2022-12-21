import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import '../../components/collection/collection-selection-actions.element';
import '../../components/collection/collection-toolbar.element';
import { IRoutingInfo } from 'router-slot';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import type { ManifestCollectionView } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { UmbDashboardMediaManagementElement } from 'src/backoffice/dashboards/media-management/dashboard-media-management.element';

@customElement('umb-collection-view-media')
export class UmbCollectionViewMediaElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}
		`,
	];

	@state()
	private _routes: Array<any> = [];

	@state()
	private _collectionViews: Array<ManifestCollectionView> = [];

	@state()
	private _currentLayoutPathName = '';

	@state()
	private _selection: Array<string> = [];

	private _mediaContext?: UmbDashboardMediaManagementElement;

	constructor() {
		super();

		this._observeCollectionViews();
		this.consumeAllContexts(['umbMediaContext'], (instance) => {
			this._mediaContext = instance['umbMediaContext'];
			this._observeMediaContext();
		});
	}

	private _observeMediaContext() {
		if (!this._mediaContext) return;

		this.observe<Array<string>>(this._mediaContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _observeCollectionViews() {
		this.observe<Array<ManifestCollectionView>>(
			umbExtensionsRegistry?.extensionsOfType('collectionView').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.meta.entityType === 'media');
				})
			),
			(layouts) => {
				if (layouts?.length === 0) return;
				this._collectionViews = layouts;
				this._createRoutes();
			}
		);
	}

	private _createRoutes() {
		this._routes = [];

		this._routes = this._collectionViews.map((layout) => {
			return {
				path: `${layout.meta.pathName}`,
				component: () => createExtensionElement(layout),
				setup: (_element: ManifestCollectionView, info: IRoutingInfo) => {
					this._currentLayoutPathName = info.match.route.path;
				},
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: this._collectionViews?.[0]?.meta.pathName,
		});
	}

	render() {
		return html`
			<umb-body-layout>
				<umb-collection-toolbar slot="header"></umb-collection-toolbar>
				<router-slot id="router-slot" .routes="${this._routes}"></router-slot>
				${this._selection.length > 0
					? html`<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>`
					: nothing}
			</umb-body-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media': UmbCollectionViewMediaElement;
	}
}
