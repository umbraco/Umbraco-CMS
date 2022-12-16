import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { BehaviorSubject, Observable } from 'rxjs';
import type { IRoute, IRoutingInfo } from 'router-slot';
import { UmbWorkspaceEntityElement } from '../../../../workspaces/shared/workspace-entity/workspace-entity.element';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';

import './list-view-layouts/table/workspace-view-users-table.element';
import './list-view-layouts/grid/workspace-view-users-grid.element';
import './workspace-view-users-selection.element';
import './workspace-view-users-invite.element';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbUserStore } from 'src/core/stores/user/user.store';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				height: 100%;
			}
		`,
	];

	@state()
	private _routes: IRoute[] = [
		{
			path: 'overview',
			component: () => import('./workspace-view-users-overview.element'),
		},
		{
			path: `:entityType/:key`,
			component: () => import('../../../../workspaces/shared/workspace-entity/workspace-entity.element'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbWorkspaceEntityElement;
				element.entityKey = info.match.params.key;
				element.entityType = info.match.params.entityType;
			},
		},
		{
			path: '**',
			redirectTo: 'overview',
		},
	];

	private _userStore?: UmbUserStore;

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	private _users: BehaviorSubject<Array<UserDetails>> = new BehaviorSubject(<Array<UserDetails>>[]);
	public readonly users: Observable<Array<UserDetails>> = this._users.asObservable();

	private _search: BehaviorSubject<string> = new BehaviorSubject('');
	public readonly search: Observable<string> = this._search.asObservable();

	constructor() {
		super();

		this.consumeAllContexts(['umbUserStore', 'umbUserGroupStore', 'umbUsersContext'], (instances) => {
			this._userStore = instances['umbUserStore'];
			this._observeUsers();
		});
		this.provideContext('umbUsersContext', this);
	}

	private _observeUsers() {
		if (!this._userStore) return;

		if (this._search.getValue()) {
			this.observe<Array<UserDetails>>(this._userStore.getByName(this._search.getValue()), (users) =>
				this._users.next(users)
			);
		} else {
			this.observe<Array<UserDetails>>(this._userStore.getAll(), (users) => this._users.next(users));
		}
	}

	public setSearch(value: string) {
		if (!value) value = '';

		this._search.next(value);
		this._observeUsers();
		this.requestUpdate('search');
	}

	public setSelection(value: Array<string>) {
		if (!value) return;
		this._selection.next(value);
		this.requestUpdate('selection');
	}

	public select(key: string) {
		const selection = this._selection.getValue();
		this._selection.next([...selection, key]);
		this.requestUpdate('selection');
	}

	public deselect(key: string) {
		const selection = this._selection.getValue();
		this._selection.next(selection.filter((k) => k !== key));
		this.requestUpdate('selection');
	}

	render() {
		return html`<router-slot .routes=${this._routes}></router-slot>`;
	}
}

export default UmbSectionViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-users': UmbSectionViewUsersElement;
	}
}
