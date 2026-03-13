import { UMB_CURRENT_USER_MODAL } from './modals/current-user/current-user-modal.token.js';
import type { UmbCurrentUserModel } from './types.js';
import { UMB_CURRENT_USER_CONTEXT } from './constants.js';
import { getInitials } from './utils/index.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-current-user-header-app')
export class UmbCurrentUserHeaderAppElement extends UmbHeaderAppButtonElement {
	@state()
	private _currentUser?: UmbCurrentUserModel;

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
			},
			'umbCurrentUserObserver',
		);
	}

	async #handleUserClick() {
		await umbOpenModal(this, UMB_CURRENT_USER_MODAL).catch(() => undefined);
	}

	override render() {
		const userInitials = this._currentUser?.name ? getInitials(this._currentUser.name) : '';
		const accessibleLabel = this._currentUser?.name
			? this.localize.term('visuallyHiddenTexts_profileOptions', this._currentUser.name, userInitials)
			: this.localize.term('visuallyHiddenTexts_profileOptionsDefault');

		return html`
			<uui-button
				@click=${this.#handleUserClick}
				look="primary"
				label=${accessibleLabel}
				compact>
				<umb-user-avatar
					id="Avatar"
					.name=${this._currentUser?.name}
					.imgUrls=${this._currentUser?.avatarUrls || []}></umb-user-avatar>
			</uui-button>
		`;
	}

	static override readonly styles = [
		...UmbHeaderAppButtonElement.styles,
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
