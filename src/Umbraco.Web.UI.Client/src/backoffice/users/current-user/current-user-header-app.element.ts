import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbCurrentUserStore } from './current-user.store';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbModalService } from 'src/core/modal';
import { UmbLitElement } from 'src/core/element/lit-element.element';

@customElement('umb-current-user-header-app')
export class UmbCurrentUserHeaderApp extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 14px;
			}
		`,
	];

	@state()
	private _currentUser?: UserDetails | null;

	private _currentUserStore?: UmbCurrentUserStore;
	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeAllContexts(['umbCurrentUserStore', 'umbModalService'], (instances) => {
			this._currentUserStore = instances['umbCurrentUserStore'];
			this._modalService = instances['umbModalService'];
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		if (!this._currentUserStore) return;

		this.observe<UserDetails>(this._currentUserStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private _handleUserClick() {
		this._modalService?.userSettings();
	}

	render() {
		return html`
			<uui-button @click=${this._handleUserClick} look="primary" label="${this._currentUser?.name || ''}" compact>
				<uui-avatar name="${this._currentUser?.name || ''}"></uui-avatar>
			</uui-button>
		`;
	}
}

export default UmbCurrentUserHeaderApp;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-header-app': UmbCurrentUserHeaderApp;
	}
}
