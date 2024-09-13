import type { UmbUserDisplayStatus } from '../../../../utils.js';
import { TimeFormatOptions, getDisplayStateFromUserStatus } from '../../../../utils.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context-token.js';
import type { UmbUserDetailModel } from '../../../../types.js';
import { UmbUserKind } from '../../../../utils/index.js';
import { html, customElement, state, css, repeat, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

type UmbUserWorkspaceInfoItem = { labelKey: string; value: string | number | undefined };

@customElement('umb-user-workspace-info')
export class UmbUserWorkspaceInfoElement extends UmbLitElement {
	@state()
	private _userInfo: Array<UmbUserWorkspaceInfoItem> = [];

	@state()
	private _userDisplayState: UmbUserDisplayStatus | null = null;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(
				this.#userWorkspaceContext.data,
				async (user) => {
					if (!user) return;
					this.#setUserInfoItems(user);
					this._userDisplayState = user.state ? getDisplayStateFromUserStatus(user.state) : null;
				},
				'umbUserObserver',
			);
		});
	}

	#setUserInfoItems = (user: UmbUserDetailModel | undefined) => {
		if (!user) {
			this._userInfo = [];
			return;
		}

		this._userInfo = [
			{
				labelKey: 'user_kind',
				value:
					user.kind === UmbUserKind.API
						? this.localize.term('user_userKindApi')
						: this.localize.term('user_userKindDefault'),
			},
			{
				labelKey: 'user_lastLogin',
				value: user.lastLoginDate
					? this.localize.date(user.lastLoginDate, TimeFormatOptions)
					: `${user.name} ${this.localize.term('user_noLogin')}`,
			},
			{ labelKey: 'user_failedPasswordAttempts', value: user.failedLoginAttempts },
			{
				labelKey: 'user_lastLockoutDate',
				value: user.lastLockoutDate
					? this.localize.date(user.lastLockoutDate, TimeFormatOptions)
					: `${user.name} ${this.localize.term('user_noLockouts')}`,
			},
			{
				labelKey: 'user_lastPasswordChangeDate',
				value: user.lastPasswordChangeDate
					? this.localize.date(user.lastPasswordChangeDate, TimeFormatOptions)
					: `${user.name} ${this.localize.term('user_noPasswordChange')}`,
			},
			{ labelKey: 'user_createDate', value: this.localize.date(user.createDate!, TimeFormatOptions) },
			{ labelKey: 'user_updateDate', value: this.localize.date(user.updateDate!, TimeFormatOptions) },
			{ labelKey: 'general_id', value: user.unique },
		];

		if (user.kind === UmbUserKind.API) {
			const include = ['user_kind', 'user_createDate', 'user_updateDate', 'general_id'];
			this._userInfo = this._userInfo.filter((item) => include.includes(item.labelKey));
		}
	};

	override render() {
		if (this._userInfo.length === 0) return nothing;
		return html`<uui-box id="user-info">${this.#renderState()} ${this.#renderInfoList()} </uui-box>`;
	}

	#renderState() {
		return html`
			<div id="state" class="user-info-item">
				<uui-tag look="${ifDefined(this._userDisplayState?.look)}" color="${ifDefined(this._userDisplayState?.color)}">
					${this.localize.term('user_' + this._userDisplayState?.key)}
				</uui-tag>
			</div>
		`;
	}

	#renderInfoList() {
		return html`
			<umb-stack look="compact">
				${repeat(
					this._userInfo,
					(item) => item.labelKey,
					(item) => this.#renderInfoItem(item.labelKey, item.value),
				)}
			</umb-stack>
		`;
	}

	#renderInfoItem(labelKey: string, value?: string | number) {
		return html`
			<div>
				<h4><umb-localize key=${labelKey}></umb-localize></h4>
				<span>${value}</span>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			uui-tag {
				width: fit-content;
			}

			h4 {
				margin: 0;
			}

			#state {
				border-bottom: 1px solid var(--uui-color-divider);
				padding-bottom: var(--uui-size-space-4);
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
