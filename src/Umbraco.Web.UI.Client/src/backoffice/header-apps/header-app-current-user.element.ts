import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbModalService } from '@umbraco-cms/services';
import { umbCurrentUserService } from 'src/core/services/current-user';

@customElement('umb-header-app-current-user')
export class UmbHeaderAppCurrentUser extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 14px;
			}
		`,
	];

	@state()
	private _currentUser?: UserDetails;

	private _modalService?: UmbModalService;

	constructor() {
		super();
		this.consumeAllContexts(['umbUserStore', 'umbModalService'], (instances) => {
			this._modalService = instances['umbModalService'];
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		this.observe<UserDetails>(umbCurrentUserService.currentUser, (currentUser) => {
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

export default UmbHeaderAppCurrentUser;

declare global {
	interface HTMLElementTagNameMap {
		'umb-header-app-current-user': UmbHeaderAppCurrentUser;
	}
}
