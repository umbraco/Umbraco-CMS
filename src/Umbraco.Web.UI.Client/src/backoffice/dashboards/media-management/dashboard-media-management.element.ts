import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import '../../components/collection/collection-toolbar.element';
import '../../components/collection/collection-selection-actions.element';
import '../../components/collection/collection-view.element';
import { map } from 'rxjs';
import { IRoutingInfo } from 'router-slot';
import type { ManifestCollectionLayout } from '@umbraco-cms/models';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { createExtensionElement } from '@umbraco-cms/extensions-api';

@customElement('umb-dashboard-media-management')
export class UmbDashboardMediaManagementElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
				border: 1px solid black;
			}
		`,
	];

	@state()
	private _routes: Array<any> = [];

	@state()
	private _collectionLayouts: Array<ManifestCollectionLayout> = [];

	@state()
	private _currentLayoutPathName = '';

	constructor() {
		super();
		this.provideContext('umbMediaContext', this);
		this._observeCollectionLayouts();
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
				<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>
			</umb-collection-view>
		`;
	}
}

export default UmbDashboardMediaManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-media-management': UmbDashboardMediaManagementElement;
	}
}
