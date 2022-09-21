import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../../../core/context';
import { BehaviorSubject, Observable } from 'rxjs';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';
import './editor-view-users-table.element';
import './editor-view-users-grid.element';
import './editor-view-users-selection.element';
import './editor-view-users-user-details.element';
import { IRoute } from 'router-slot';

@customElement('umb-editor-view-users')
export class UmbEditorViewUsersElement extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _routes: IRoute[] = [
		{
			path: 'list',
			component: () => import('./editor-view-users-list.element'),
		},
		{
			path: 'details/:key',
			component: () => import('./editor-view-users-user-details.element'),
		},
	];

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
