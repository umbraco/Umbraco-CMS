import { UMB_CURRENT_USER_CONTEXT } from '../../current-user.context.token.js';
import type { UmbCurrentUserEditProfileAvatarElement } from './edit-profile-avatar.element.js';
import type { UmbCurrentUserEditProfileSettingsElement } from './edit-profile-settings.element.js';
import { css, customElement, html, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import './edit-profile-avatar.element.js';
import './edit-profile-settings.element.js';

@customElement('umb-current-user-edit-profile-modal')
export class UmbCurrentUserEditProfileModalElement extends UmbModalBaseElement {
	@state()
	private _headline?: string;

	@query('umb-current-user-edit-profile-avatar')
	private _avatarEl?: UmbCurrentUserEditProfileAvatarElement;

	@query('umb-current-user-edit-profile-settings')
	private _profileSettingsEl?: UmbCurrentUserEditProfileSettingsElement;

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context?.currentUser,
				(currentUser) => (this._headline = currentUser?.name),
				'umbCurrentUserObserver',
			);
		});
	}

	protected override async _submitModal() {
		await this._avatarEl?.save();
		await this._profileSettingsEl?.save();
		this.modalContext?.submit();
		super._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this._headline ?? ''}>
				<umb-stack look="compact">
					<umb-current-user-edit-profile-avatar></umb-current-user-edit-profile-avatar>
					<umb-current-user-edit-profile-settings></umb-current-user-edit-profile-settings>
				</umb-stack>
				<uui-button
					slot="actions"
					look="secondary"
					label=${this.localize.term('general_close')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					slot="actions"
					color="positive"
					look="primary"
					label=${this.localize.term('buttons_save')}
					@click=${this._submitModal}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export default UmbCurrentUserEditProfileModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-edit-profile-modal': UmbCurrentUserEditProfileModalElement;
	}
}
