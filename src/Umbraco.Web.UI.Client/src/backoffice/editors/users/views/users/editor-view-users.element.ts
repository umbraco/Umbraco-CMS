import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextProviderMixin } from '../../../../../core/context';
import { BehaviorSubject, Observable } from 'rxjs';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';
import { IRoute } from 'router-slot';
import './editor-view-users-table.element';
import './editor-view-users-grid.element';
import './editor-view-users-selection.element';
import './editor-view-users-user-details.element';

import { tempData } from './tempData';

export interface UserItem {
	id: number;
	key: string;
	name: string;
	email: string;
	status: string;
	language: string;
	lastLoginDate?: string;
	lastLockoutDate?: string;
	lastPasswordChangeDate?: string;
	updateDate: string;
	createDate: string;
	failedLoginAttempts: number;
	userGroup?: string; //TODO Implement this
}

@customElement('umb-editor-view-users')
export class UmbEditorViewUsersElement extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _routes: IRoute[] = [
		{
			path: '/',
			component: () => import('./editor-view-users-list.element'),
		},
		{
			path: '/:key',
			component: () => import('./editor-view-users-user-details.element'),
		},
	];

	private _users: BehaviorSubject<Array<UserItem>> = new BehaviorSubject(tempData);
	public readonly users: Observable<Array<UserItem>> = this._users.asObservable();

	private _selection: BehaviorSubject<Array<string>> = new BehaviorSubject(<Array<string>>[]);
	public readonly selection: Observable<Array<string>> = this._selection.asObservable();

	constructor() {
		super();

		this.provideContext('umbUsersContext', this);
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

	public updateUser(user: UserItem) {
		const users = this._users.getValue();
		const index = users.findIndex((u) => u.key === user.key);
		if (index === -1) return;
		users[index] = { ...users[index], ...user };
		console.log('updateUser', user, users[index]);
		this._users.next(users);
		this.requestUpdate('users');
	}

	public deleteUser(key: string) {
		const users = this._users.getValue();
		const index = users.findIndex((u) => u.key === key);
		if (index === -1) return;
		users.splice(index, 1);
		this._users.next(users);
		this.requestUpdate('users');
	}

	public getTagLookAndColor(status?: string): { color: InterfaceColor; look: InterfaceLook } {
		switch ((status || '').toLowerCase()) {
			case 'invited':
			case 'inactive':
				return { look: 'primary', color: 'warning' };
			case 'active':
				return { look: 'primary', color: 'positive' };
			case 'disabled':
				return { look: 'primary', color: 'danger' };
			default:
				return { look: 'secondary', color: 'default' };
		}
	}

	render() {
		return html` <router-slot .routes=${this._routes}></router-slot> `;
	}
}

export default UmbEditorViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-users': UmbEditorViewUsersElement;
	}
}
