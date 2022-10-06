import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';
import { IRoute, IRoutingInfo } from 'router-slot';
import { v4 as uuidv4 } from 'uuid';
import { UmbContextProviderMixin } from '../../../../../core/context';
import './list-view-layouts/table/editor-view-users-table.element';
import './list-view-layouts/grid/editor-view-users-grid.element';
import './editor-view-users-selection.element';
import './editor-view-users-invite.element';

import type { UserDetails, UserEntity } from '../../../../../core/models';
import type { UmbEditorUserElement } from '../../../../editors/user/editor-user.element';
import UmbEditorEntityElement from '../../../../editors/shared/editor-entity/editor-entity.element';

@customElement('umb-section-view-users')
export class UmbSectionViewUsersElement extends UmbContextProviderMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@state()
	private _routes: IRoute[] = [
		{
			path: 'overview',
			component: () => import('./editor-view-users-overview.element'),
		},
		{
			path: 'invite',
			component: () => import('./editor-view-users-invite.element'),
		},
		{
			path: `:entityType/:key`,
			component: () => import('../../../../editors/shared/editor-entity/editor-entity.element'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbEditorEntityElement;
				element.entityKey = info.match.params.key;
				element.entityType = info.match.params.entityType;
			},
		},
		{
			path: '**',
			redirectTo: '/section/users/view/users/overview', //TODO: this should be dynamic
		},
	];

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

	render() {
		return html` <router-slot .routes=${this._routes}></router-slot> `;
	}
}

export default UmbSectionViewUsersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-view-users': UmbSectionViewUsersElement;
	}
}
