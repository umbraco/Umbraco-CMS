import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

import '../collection/views/table/user-table-collection-view.element';
import '../collection/views/grid/user-grid-collection-view.element';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbLitElement {
	#routes: UmbRoute[] = [
		{
			path: 'collection',
			component: () => import('../collection/user-collection.element'),
		},
		{
			path: 'user',
			component: () => import('../workspace/user-workspace.element'),
		},
		{
			path: '**',
			redirectTo: 'collection',
		},
	];

	// // TODO: This must be turned into context api: Maybe its a Collection View (SectionView Collection View)?
	// private _userStore?: UmbUserStore;

	// #users = new DeepState(<Array<UserDetails>>[]);
	// public readonly users = this.#users.asObservable();

	// #search = new DeepState('');
	// public readonly search = this.#search.asObservable();

	// constructor() {
	// 	super();

	// 	this.consumeContext<UmbUserStore>(UMB_USER_STORE_CONTEXT_TOKEN, (_instance) => {
	// 		this._userStore = _instance;
	// 		this._observeUsers();
	// 	});
	// }

	// private _observeUsers() {
	// 	if (!this._userStore) return;

	// 	if (this.#search.getValue()) {
	// 		this.observe(this._userStore.getByName(this.#search.getValue()), (users) => this.#users.next(users));
	// 	} else {
	// 		this.observe(this._userStore.getAll(), (users) => this.#users.next(users));
	// 	}
	// }

	// public setSearch(value?: string) {
	// 	this.#search.next(value || '');
	// 	this._observeUsers();
	// 	this.requestUpdate('search');
	// }

	render() {
		return html`<umb-router-slot id="router-slot" .routes=${this.#routes}></umb-router-slot>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
			}

			#router-slot {
				height: calc(100% - var(--umb-header-layout-height));
			}
		`,
	];
}

export default UmbSectionViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-users': UmbSectionViewUsersElement;
	}
}
