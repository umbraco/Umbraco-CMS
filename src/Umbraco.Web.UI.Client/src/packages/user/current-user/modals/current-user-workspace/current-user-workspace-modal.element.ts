import type { UmbCurrentUserModel } from '../../types.js';
import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserWorkspaceProfileSettingsElement } from './current-user-workspace-profile-settings.element.js';
import type { UmbCurrentUserWorkspaceAvatarElement } from './current-user-workspace-avatar.element.js';
import { css, customElement, html, property, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

import './current-user-workspace-avatar.element.js';
import './current-user-workspace-profile-settings.element.js';
@customElement('umb-current-user-workspace-modal')
export class UmbCurrentUserWorkspaceModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	@query('umb-current-user-workspace-avatar')
	private _avatar?: UmbCurrentUserWorkspaceAvatarElement;

	@query('umb-current-user-workspace-profile-settings')
	private _profileSettings?: UmbCurrentUserWorkspaceProfileSettingsElement;

	@state()
	private _currentUser?: UmbCurrentUserModel;

	private _close() {
		this.modalContext?.reject();
	}

	private async _save() {
		await this._profileSettings?.save();
		await this._avatar?.save();
		this.modalContext?.submit();
	}

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

	override render() {
		return html`
			<umb-body-layout headline="${this._currentUser?.name || ''}">
				<div id="main">
					<umb-stack>
						<umb-current-user-workspace-avatar></umb-current-user-workspace-avatar>
						<umb-current-user-workspace-profile-settings></umb-current-user-workspace-profile-settings>
					</umb-stack>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary" .label=${this.localize.term('general_close')}>
						${this.localize.term('general_close')}
					</uui-button>
					<uui-button @click=${this._save} look="primary" color="positive" .label=${this.localize.term('buttons_save')}>
						${this.localize.term('buttons_save')}
					</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbCurrentUserWorkspaceModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-workspace-modal': UmbCurrentUserWorkspaceModalElement;
	}
}
