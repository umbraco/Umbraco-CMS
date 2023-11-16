import { getDisplayStateFromUserStatus } from '../../../../utils.js';
import { UMB_USER_WORKSPACE_CONTEXT } from '../../user-workspace.context.js';
import { UmbUserDetail } from '../../../types.js';
import {
	html,
	customElement,
	state,
	css,
	repeat,
	ifDefined,
	query,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

type UmbUserWorkspaceInfoItem = { labelKey: string; value: string | number | undefined };

@customElement('umb-user-workspace-info')
export class UmbUserWorkspaceInfoElement extends UmbLitElement {
	@state()
	private _user?: UmbUserDetail;

	@state()
	private _userAvatarUrls: Array<{ url: string; scale: string }> = [];

	@state()
	private _userInfo: Array<UmbUserWorkspaceInfoItem> = [];

	@query('#AvatarFileField')
	_avatarFileField?: HTMLInputElement;

	#userWorkspaceContext?: typeof UMB_USER_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_WORKSPACE_CONTEXT, (instance) => {
			this.#userWorkspaceContext = instance;
			this.observe(
				this.#userWorkspaceContext.data,
				async (user) => {
					this._user = user;
					this.#setUserAvatarUrls(user);
					this.#setUserInfoItems(user);
				},
				'umbUserObserver',
			);
		});
	}

	async #getAppContext() {
		// TODO: remove this when we get absolute urls from the server
		return this.consumeContext(UMB_APP_CONTEXT, (instance) => {}).asPromise();
	}

	// TODO: remove this when we get absolute urls from the server
	#setUserAvatarUrls = async (user: UmbUserDetail | undefined) => {
		if (user?.avatarUrls?.length === 0) return;

		const serverUrl = (await this.#getAppContext()).getServerUrl();
		if (!serverUrl) return;

		this._userAvatarUrls = [
			{
				scale: '1x',
				url: `${serverUrl}${user?.avatarUrls?.[3]}`,
			},
			{
				scale: '2x',
				url: `${serverUrl}${user?.avatarUrls?.[4]}`,
			},
		];
	};

	#onAvatarUploadSubmit = (event: SubmitEvent) => {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		if (!form.checkValidity()) return;

		const formData = new FormData(form);

		const avatarFile = formData.get('avatarFile') as File;

		this.#userWorkspaceContext?.uploadAvatar(avatarFile);
	};

	#deleteAvatar = async () => {
		if (!this.#userWorkspaceContext) return;
		const { error } = await this.#userWorkspaceContext.deleteAvatar();

		if (!error) {
			this._userAvatarUrls = [];
		}
	};

	#setUserInfoItems = (user: UmbUserDetail | undefined) => {
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
			${this.#renderAvatar()}

			<uui-box id="user-info">
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

	#getAvatarSrcset() {
		let string = '';

		this._userAvatarUrls?.forEach((url) => {
			string += `${url.url} ${url.scale},`;
		});
		return string;
	}

	#hasAvatar() {
		return this._userAvatarUrls.length > 0;
	}

	#renderAvatar() {
		return html`
			<uui-box>
				<div id="user-avatar-settings" class="user-info-item">
					<form id="AvatarUploadForm" @submit=${this.#onAvatarUploadSubmit}>
						<uui-avatar
							id="Avatar"
							.name=${this._user?.name || ''}
							img-src=${ifDefined(this.#hasAvatar() ? this._userAvatarUrls[0].url : undefined)}
							img-srcset=${ifDefined(this.#hasAvatar() ? this.#getAvatarSrcset() : undefined)}></uui-avatar>
						(WIP)
						<input id="AvatarFileField" type="file" name="avatarFile" required />
						<uui-button type="submit" label="${this.localize.term('user_changePhoto')}"></uui-button>
						${this.#hasAvatar()
							? html`
									<uui-button
										type="button"
										label=${this.localize.term('user_removePhoto')}
										@click=${this.#deleteAvatar}></uui-button>
							  `
							: nothing}
					</form>
				</div>
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
			uui-tag {
				width: fit-content;
			}

			#Avatar {
				font-size: 75px;
				place-self: center;
			}

			#user-info {
				margin-bottom: var(--uui-size-space-4);
			}

			#user-info > .user-info-item {
				display: flex;
				flex-direction: column;
				margin-bottom: var(--uui-size-space-3);
			}

			#user-avatar-settings form {
				text-align: center;
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
