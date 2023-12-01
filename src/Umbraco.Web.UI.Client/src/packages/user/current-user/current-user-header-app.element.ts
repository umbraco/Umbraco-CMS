import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, CSSResultGroup, html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CURRENT_USER_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_CURRENT_USER_CONTEXT, type UmbCurrentUser } from '@umbraco-cms/backoffice/current-user';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';

@customElement('umb-current-user-header-app')
export class UmbCurrentUserHeaderAppElement extends UmbLitElement {
	@state()
	private _currentUser?: UmbCurrentUser;

	@state()
	private _userAvatarUrls: Array<{ url: string; scale: string }> = [];

	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#modalManagerContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});

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

	private _handleUserClick() {
		this.#modalManagerContext?.open(UMB_CURRENT_USER_MODAL);
	}

	async #getAppContext() {
		// TODO: remove this when we get absolute urls from the server
		return this.consumeContext(UMB_APP_CONTEXT, (instance) => {}).asPromise();
	}

	#setUserAvatarUrls = async (user: UmbCurrentUser | undefined) => {
		if (!user || !user.avatarUrls || user.avatarUrls.length === 0) return;

		// TODO: remove this when we get absolute urls from the server
		// TODO: temp hack because we can't prefix local urls with the server url.
		// these are preview urls for newly uploaded avatars
		const serverUrl = (await this.#getAppContext()).getServerUrl();
		if (!serverUrl) return;

		// TODO: hack to only use size 3 and 4 from the array. The server should only return 1 url.
		const isRelativeUrl = user.avatarUrls[0].startsWith('/');
		const avatarScale1 = user.avatarUrls?.[0];
		const avatarScale2 = user.avatarUrls?.[1];
		const avatarScale3 = user.avatarUrls?.[2];

		this._userAvatarUrls = [
			{
				scale: '1x',
				url: isRelativeUrl ? serverUrl + avatarScale1 : avatarScale1,
			},
			{
				scale: '2x',
				url: isRelativeUrl ? serverUrl + avatarScale2 : avatarScale2,
			},
			{
				scale: '3x',
				url: isRelativeUrl ? serverUrl + avatarScale3 : avatarScale3,
			},
		];
	};

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

	render() {
		return html`
			<uui-button
				@click=${this._handleUserClick}
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

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			uui-button {
				font-size: 14px;
				--uui-button-background-color: transparent;
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
