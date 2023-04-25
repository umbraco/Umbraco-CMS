import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UMB_COLLECTION_CONTEXT_TOKEN } from '../../../shared/components/collection/collection.context';
import { UmbUserCollectionContext } from './user-collection.context';
import type { IRoute } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

import './views/table/user-collection-table-view.element';
import './views/grid/user-collection-grid-view.element';
import './user-collection-header.element';

export type UsersViewType = 'list' | 'grid';
@customElement('umb-user-collection')
export class UmbUserCollectionElement extends UmbLitElement {
	#collectionContext = new UmbUserCollectionContext(this);

	@state()
	private _routes: IRoute[] = [
		{
			path: 'grid',
			component: () => import('./views/grid/user-collection-grid-view.element'),
		},
		{
			path: 'list',
			component: () => import('./views/table/user-collection-table-view.element'),
		},
		{
			path: '**',
			redirectTo: 'grid',
		},
	];

	connectedCallback(): void {
		super.connectedCallback();
		this.provideContext(UMB_COLLECTION_CONTEXT_TOKEN, this.#collectionContext);
	}

	render() {
		return html`
			<uui-scroll-container>
				<umb-user-collection-header></umb-user-collection-header>
				<umb-router-slot id="router-slot" .routes=${this._routes}></umb-router-slot>
			</uui-scroll-container>

			<umb-collection-selection-actions></umb-collection-selection-actions>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbUserCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-collection': UmbUserCollectionElement;
	}
}
