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

export interface UserItem {
	key: string;
	name: string;
	userGroup: string;
	lastLogin: string;
	status?: string;
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

	private tempData = [
		{
			key: 'a9b18a00-58f2-420e-bf60-48d33ab156db',
			name: 'Cecílie Bryon',
			userGroup: 'Translators',
			lastLogin: 'Fri, 23 April 2021',
			status: 'Invited',
		},
		{
			key: '3179d0b2-eec2-4045-b86a-149e13b93e14',
			name: 'Kathleen G. Smith',
			userGroup: 'Editors',
			lastLogin: 'Tue, 6 June 2021',
			status: 'Disabled',
		},
		{
			key: '1b1c9733-b845-4d9a-9ed2-b2f46c05fd72',
			name: 'Adrian Andresen',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0207',
			name: 'Lorenza Trentino',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0202',
			name: 'John Doe',
			userGroup: 'Translators',
			lastLogin: 'Tue, 11 December 2021',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0203',
			name: 'Jane Doe',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0204',
			name: 'John Smith',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0205',
			name: 'Jane Smith',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0206',
			name: 'Oliver Twist',
			userGroup: 'Translators',
			lastLogin: 'Tue, 11 December 2021',
		},
		{
			key: 'b75af81a-b994-4e65-2330-b66c336d0207',
			name: 'Olivia Doe',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b66c336d0208',
			name: 'Hans Hansen',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
		{
			key: 'a9b18a00-58f2-sjh2-bf60-48d33ab156db',
			name: 'Brian Adams',
			userGroup: 'Translators',
			lastLogin: 'Fri, 23 April 2021',
			status: 'Invited',
		},
		{
			key: '3179d0b2-eec2-4432-b86a-149e13b93e14',
			name: 'Smith John',
			userGroup: 'Editors',
			lastLogin: 'Tue, 6 June 2021',
			status: 'Disabled',
		},
		{
			key: '1b1c9723-b845-4d9a-9ed2-b2f46c05fd72',
			name: 'Reese Witherspoon',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
		{
			key: 'b75af81a-2f94-4e65-9330-b66c336d0207',
			name: 'Denzel Washington',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e23-9330-b66c336d0202',
			name: 'Leonardo DiCaprio',
			userGroup: 'Translators',
			lastLogin: 'Tue, 11 December 2021',
		},
		{
			key: 'b75af81a-2394-4e65-9330-b66c336d0203',
			name: 'Idris Elba',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9330-b6u7336d0204',
			name: 'Quentin Tarantino',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
		{
			key: 'b75af81a-b994-4e65-2330-c66c336d0205',
			name: 'Tom Hanks',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af82a-b994-4b65-9330-b66c336d0206',
			name: 'Oprah Winfrey',
			userGroup: 'Translators',
			lastLogin: 'Tue, 11 December 2021',
		},
		{
			key: 'b75af81a-b994-4e65-2s30-b66b336d0207',
			name: 'Pamela Anderson',
			userGroup: 'Editors',
			lastLogin: 'Fri, 13 April 2022',
		},
		{
			key: 'b75af81a-b994-4e65-9930-b66c336d0l33t',
			name: 'Keanu Reeves',
			userGroup: 'Administrators',
			lastLogin: 'Mon, 15 November 2021',
		},
	];

	private _users: BehaviorSubject<Array<UserItem>> = new BehaviorSubject(this.tempData);
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

	public getTagLookAndColor(status: string): { color: InterfaceColor; look: InterfaceLook } {
		switch (status.toLowerCase()) {
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
