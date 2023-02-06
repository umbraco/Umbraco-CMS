import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state, property } from 'lit/decorators.js';
import { map } from 'rxjs';
import './collection-selection-actions.element';
import './collection-toolbar.element';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from './collection.context';
import { createExtensionElement , umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import type { ManifestCollectionView, MediaDetails } from '@umbraco-cms/models';
import { UmbLitElement } from '@umbraco-cms/element';
import type { UmbObserverController } from '@umbraco-cms/observable-api';

@customElement('umb-collection')
export class UmbCollectionElement extends UmbLitElement {
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
			router-slot {
				width: 100%;
				height: 100%;
			}
		`,
	];

	@state()
	private _routes: Array<any> = [];

	@state()
	private _selection?: Array<string> | null;

	private _collectionContext?: UmbCollectionContext<MediaDetails>;

	private _entityType!: string;
	@property()
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		this._entityType = value;
		this._observeCollectionViews();
	}

	private _collectionViewUnsubscribe?: UmbObserverController<Array<ManifestCollectionView>>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT_TOKEN, (instance) => {
			this._collectionContext = instance;
			this._observeCollectionContext();
		});
	}

	private _observeCollectionContext() {
		if (!this._collectionContext) return;

		this.observe(this._collectionContext.selection, (selection) => {
			this._selection = selection;
		});
	}

	private _observeCollectionViews() {
		this._collectionViewUnsubscribe?.destroy();
		this._collectionViewUnsubscribe = this.observe(
			// TODO: could we make some helper methods for this scenario:
			umbExtensionsRegistry?.extensionsOfType('collectionView').pipe(
				map((extensions) => {
					return extensions.filter((extension) => extension.meta.entityType === this._entityType);
				})
			),
			(views) => {
				this._createRoutes(views);
			}
		);
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
			path: '**',
			redirectTo: views?.[0]?.meta.pathName,
		});
	}

	render() {
		return html`
			<umb-body-layout no-header-background>
				<umb-collection-toolbar slot="header"></umb-collection-toolbar>
				<umb-router-slot id="router-slot" .routes="${this._routes}"></umb-router-slot>
				${this._selection && this._selection.length > 0
					? html`<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>`
					: nothing}
			</umb-body-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
