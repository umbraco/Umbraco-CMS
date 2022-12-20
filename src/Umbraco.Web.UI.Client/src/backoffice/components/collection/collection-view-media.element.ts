import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import '../../components/collection/collection-selection-actions.element';
import '../../components/collection/collection-toolbar.element';
import '../../components/collection/collection-view.element';
import { IRoutingInfo } from 'router-slot';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import type { ManifestCollectionLayout } from '@umbraco-cms/models';
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
	private _collectionLayouts: Array<ManifestCollectionLayout> = [];

	@state()
	private _currentLayoutPathName = '';

	@state()
	private _selection: Array<string> = [];

	private _mediaContext?: UmbDashboardMediaManagementElement;

	constructor() {
		super();

		this._observeCollectionLayouts();
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

	private _observeCollectionLayouts() {
		this.observe<Array<ManifestCollectionLayout>>(
			umbExtensionsRegistry?.extensionsOfType('collectionLayout').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.meta.entityType === 'media');
				})
			),
			(layouts) => {
				if (layouts?.length === 0) return;
				this._collectionLayouts = layouts;
				this._createRoutes();
			}
		);
	}

	private _createRoutes() {
		this._routes = [];

		this._routes = this._collectionLayouts.map((layout) => {
			return {
				path: `${layout.meta.pathName}`,
				component: () => createExtensionElement(layout),
				setup: (_element: ManifestCollectionLayout, info: IRoutingInfo) => {
					this._currentLayoutPathName = info.match.route.path;
				},
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: this._collectionLayouts?.[0]?.meta.pathName,
		});
	}

	render() {
		return html`
			<umb-collection-view>
				<umb-collection-toolbar slot="header"></umb-collection-toolbar>
				<router-slot slot="main" id="router-slot" .routes="${this._routes}"></router-slot>
				${this._selection.length > 0
					? html`<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>`
					: nothing}
			</umb-collection-view>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-view-media': UmbCollectionViewMediaElement;
	}
}
