import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UserDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbModalService } from '@umbraco-cms/services';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbUserStore } from 'src/core/stores/user/user.store';

@customElement('umb-backoffice-header-tools')
export class UmbBackofficeHeaderTools extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#tools {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}

			.tool {
				font-size: 18px;
			}
		`,
	];

	constructor() {
		super();
		this.consumeAllContexts(['umbUserStore', 'umbModalService'], (instances) => {
			this._userStore = instances['umbUserStore'];
			this._modalService = instances['umbModalService'];
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		if (!this._userStore) return;

		this.observe<UserDetails>(this._userStore.currentUser, (currentUser) => {
			this._currentUser = currentUser;
		});
	}

	private _handleUserClick() {
		this._modalService?.userSettings();
	}

	@state()
	private _currentUser?: UserDetails;

	private _userStore?: UmbUserStore;
	private _modalService?: UmbModalService;

	render() {
		return html`
			<div id="tools">
				<uui-button class="tool" look="primary" label="Search" compact>
					<uui-icon name="search"></uui-icon>
				</uui-button>
				<uui-button class="tool" look="primary" label="Help" compact>
					<uui-icon name="favorite"></uui-icon>
				</uui-button>
				<uui-button @click=${this._handleUserClick} look="primary" style="font-size: 14px;" label="User" compact>
					<uui-avatar name="${this._currentUser?.name || ''}"></uui-avatar>
				</uui-button>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-tools': UmbBackofficeHeaderTools;
	}
}
