import { getDisplayStateFromUserStatus } from '../../../../utils.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context.js';
import { html, customElement, state, css, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';

type UmbUserWorkspaceInfoItem = { labelKey: string; value: string | number | undefined };

@customElement('umb-user-workspace-info')
export class UmbUserWorkspaceInfoElement extends UmbLitElement {
	@state()
	private _user?: UserResponseModel;

	@state()
	private _userInfo: Array<UmbUserWorkspaceInfoItem> = [];

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(this.#userWorkspaceContext.data, (user) => {
				this._user = user;
				this.#setUserInfoItems(user);
			}, 'umbUserObserver');
		});
	}

	#setUserInfoItems = (user: UserResponseModel | undefined) => {
		if (!user) {
			this._userInfo = [];
			return;
		}

		this._userInfo = [
			{
				labelKey: 'user_lastLogin',
				value: user.lastLoginDate
					? this.localize.date(user.lastLoginDate)
					: `${user.name + ' ' + this.localize.term('user_noLogin')} `,
			},
			{ labelKey: 'user_failedPasswordAttempts', value: user.failedLoginAttempts },
			{
				labelKey: 'user_lastLockoutDate',
				value: user.lastLockoutDate
					? this.localize.date(user.lastLockoutDate)
					: `${user.name + ' ' + this.localize.term('user_noLockouts')}`,
			},
			{
				labelKey: 'user_lastPasswordChangeDate',
				value: user.lastPasswordChangeDate
					? this.localize.date(user.lastPasswordChangeDate)
					: `${user.name + ' ' + this.localize.term('user_noPasswordChange')}`,
			},
			{ labelKey: 'user_createDate', value: this.localize.date(user.createDate!) },
			{ labelKey: 'user_updateDate', value: this.localize.date(user.updateDate!) },
			{ labelKey: 'general_id', value: user.id },
		];
	};

	render() {
		if (!this._user) return html`User not found`;

		const displayState = getDisplayStateFromUserStatus(this._user.state);

		return html`
			<uui-box id="user-info">
				<div id="user-avatar-settings" class="user-info-item">
					<uui-avatar .name=${this._user?.name || ''}></uui-avatar>
					<uui-button label=${this.localize.term('user_changePhoto')}></uui-button>
				</div>

				<div id="user-status-info" class="user-info-item">
					<b><umb-localize key="general_status">Status</umb-localize>:</b>
					<uui-tag look="${ifDefined(displayState?.look)}" color="${ifDefined(displayState?.color)}">
						${this.localize.term('user_' + displayState.key)}
					</uui-tag>
				</div>

				${repeat(
					this._userInfo,
					(item) => item.labelKey,
					(item) => this.#renderInfoItem(item.labelKey, item.value),
				)}
			</uui-box>
		`;
	}

	#renderInfoItem(labelKey: string, value?: string | number) {
		return html`
			<div class="user-info-item">
				<b><umb-localize key=${labelKey}></umb-localize></b>
				<span>${value}</span>
			</div>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-avatar {
				font-size: var(--uui-size-16);
				place-self: center;
			}

			uui-tag {
				width: fit-content;
			}

			#user-info {
				margin-bottom: var(--uui-size-space-4);
			}

			#user-info > .user-info-item {
				display: flex;
				flex-direction: column;
				margin-bottom: var(--uui-size-space-3);
			}

			#user-avatar-settings {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbUserWorkspaceInfoElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-workspace-info': UmbUserWorkspaceInfoElement;
	}
}
