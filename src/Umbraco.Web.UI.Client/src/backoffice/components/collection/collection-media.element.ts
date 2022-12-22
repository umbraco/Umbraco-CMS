import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, state, property } from 'lit/decorators.js';
import { map } from 'rxjs';
import './collection-selection-actions.element';
import './collection-toolbar.element';
import { IRoutingInfo } from 'router-slot';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import type { ManifestCollectionView } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { UmbDashboardMediaManagementElement } from 'src/backoffice/dashboards/media-management/dashboard-media-management.element';

@customElement('umb-collection-media')
export class UmbCollectionMediaElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
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
	private _selection: Array<string> = [];

	private _mediaContext?: UmbDashboardMediaManagementElement;


	private _entityType!: string;
	@property()
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		this._entityType = value;
		this._observeCollectionViews();
	}

	private _collectionViewUnsubscribe?:() => void;


	constructor() {
		super();

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
		
		this._collectionViewUnsubscribe?.();
		this._collectionViewUnsubscribe = this.observe<Array<ManifestCollectionView>>(
			umbExtensionsRegistry?.extensionsOfType('collectionView').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.meta.entityType === this._entityType);
				})
			),
			(views) => {
				if (views?.length === 0) return;
				this._createRoutes(views);
			}
		);
	}

	private _createRoutes(views: ManifestCollectionView[]) {
		this._routes = [];

		this._routes = views.map((view) => {
			return {
				path: `${view.meta.pathName}`,
				component: () => createExtensionElement(view)
			};
		});

		this._routes.push({
			path: '**',
			redirectTo: views?.[0]?.meta.pathName,
		});
	}

	render() {
		return html`
			<umb-body-layout no-header-background>
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
		'umb-collection-media': UmbCollectionMediaElement;
	}
}
