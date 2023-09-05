import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { css, html, nothing, customElement, state, property } from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbCollectionContext, UMB_COLLECTION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/collection';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { ManifestCollectionView, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

import './collection-selection-actions.element.js';
import './collection-toolbar.element.js';

@customElement('umb-collection')
export class UmbCollectionElement extends UmbLitElement {
	@state()
	private _routes: Array<UmbRoute> = [];

	@state()
	private _selection?: Array<string> | null;

	private _collectionContext?: UmbCollectionContext<any, any>;

	private _entityType!: string;
	@property({ type: String, attribute: 'entity-type' })
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
					return extensions.filter((extension) => extension.conditions.entityType === this._entityType);
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
			path: '',
			redirectTo: views?.[0]?.meta.pathName ?? '/',
		});
	}

	render() {
		return html`
			<umb-body-layout>
				<umb-collection-toolbar slot="header"></umb-collection-toolbar>
				<umb-router-slot id="router-slot" .routes="${this._routes}"></umb-router-slot>
				${this._selection && this._selection.length > 0
					? html`<umb-collection-selection-actions slot="footer-info"></umb-collection-selection-actions>`
					: nothing}
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
