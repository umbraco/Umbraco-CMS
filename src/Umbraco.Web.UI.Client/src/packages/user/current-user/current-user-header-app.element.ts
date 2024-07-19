import { UMB_CURRENT_USER_MODAL } from './modals/current-user/current-user-modal.token.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_CURRENT_USER_CONTEXT, type UmbCurrentUserModel } from '@umbraco-cms/backoffice/current-user';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-current-user-header-app')
export class UmbCurrentUserHeaderAppElement extends UmbHeaderAppButtonElement {
	@state()
	private _currentUser?: UmbCurrentUserModel;

	@state()
	private _userAvatarUrls: Array<{ url: string; descriptor: string }> = [];

	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			this._observeCurrentUser();
		});
	}

	private async _observeCurrentUser() {
		if (!this.#currentUserContext) return;

		this.observe(
			this.#currentUserContext.currentUser,
			(currentUser) => {
				this._currentUser = currentUser;
				if (!currentUser) return;
				this.#setUserAvatarUrls(currentUser);
			},
			'umbCurrentUserObserver',
		);
	}

	async #handleUserClick() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_CURRENT_USER_MODAL);
	}

	#setUserAvatarUrls = async (user: UmbCurrentUserModel | undefined) => {
		if (!user || !user.avatarUrls || user.avatarUrls.length === 0) {
			this._userAvatarUrls = [];
			return;
		}

		this._userAvatarUrls = [
			{
				descriptor: '1x',
				url: user.avatarUrls?.[0],
			},
			{
				descriptor: '2x',
				url: user.avatarUrls?.[1],
			},
			{
				descriptor: '3x',
				url: user.avatarUrls?.[2],
			},
		];
	};

	#getAvatarSrcset() {
		let string = '';

		this._userAvatarUrls?.forEach((url) => {
			string += `${url.url} ${url.descriptor},`;
		});
		return string;
	}

	#hasAvatar() {
		return this._userAvatarUrls.length > 0;
	}

	override render() {
		return html`
			<uui-button
				@click=${this.#handleUserClick}
				look="primary"
				label="${this.localize.term('visuallyHiddenTexts_openCloseBackofficeProfileOptions')}"
				compact>
				<uui-avatar
					id="Avatar"
					.name=${this._currentUser?.name || 'Unknown'}
					img-src=${ifDefined(this.#hasAvatar() ? this._userAvatarUrls[0].url : undefined)}
					img-srcset=${ifDefined(this.#hasAvatar() ? this.#getAvatarSrcset() : undefined)}></uui-avatar>
			</uui-button>
		`;
	}

	static override styles: CSSResultGroup = [
		UmbHeaderAppButtonElement.styles,
		css`
			uui-button {
				font-size: 14px;
			}
		`,
	];
}

export default UmbCurrentUserHeaderAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-header-app': UmbCurrentUserHeaderAppElement;
	}
}
