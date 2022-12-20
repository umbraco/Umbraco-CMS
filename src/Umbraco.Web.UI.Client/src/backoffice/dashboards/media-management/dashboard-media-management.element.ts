import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import '../../components/collection/collection-toolbar.element';
import '../../components/collection/collection-selection-actions.element';
import '../../components/collection/collection-view.element';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { IRoutingInfo } from 'router-slot';
import type { ManifestCollectionLayout, MediaDetails } from '@umbraco-cms/models';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { UmbMediaStore } from '@umbraco-cms/stores/media/media.store';

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

	@property()
	public entityKey = '';

	@state()
	private _collectionLayouts: Array<ManifestCollectionLayout> = [];

	@state()
	private _currentLayoutPathName = '';

	private _mediaStore?: UmbMediaStore;

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	private _mediaItems: BehaviorSubject<Array<MediaDetails>> = new BehaviorSubject(<Array<MediaDetails>>[]);
	public readonly mediaItems: Observable<Array<MediaDetails>> = this._mediaItems.asObservable();

	private _search: BehaviorSubject<string> = new BehaviorSubject('');
	public readonly search: Observable<string> = this._search.asObservable();

	constructor() {
		super();
		this._observeCollectionLayouts();
		this.provideContext('umbMediaContext', this);
		this.consumeAllContexts(['umbMediaStore'], (instance) => {
			this._mediaStore = instance['umbMediaStore'];
			this._observeMediaItems();
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

	private _observeMediaItems() {
		if (!this._mediaStore) return;

		if (this.entityKey) {
			this.observe<Array<MediaDetails>>(this._mediaStore?.getTreeItemChildren(this.entityKey), (items) => {
				this._mediaItems.next(items);
			});
		} else {
			this.observe<Array<MediaDetails>>(this._mediaStore?.getTreeRoot(), (items) => {
				this._mediaItems.next(items);
			});
		}
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
