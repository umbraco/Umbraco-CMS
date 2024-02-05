import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import type { UmbUserItemModel } from '../../repository/item/types.js';
import type {
	UmbCreateUserSuccessModalData,
	UmbCreateUserSuccessModalValue,
} from './create-user-success-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputPasswordElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbNotificationDefaultData, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-create-user-success-modal')
export class UmbCreateUserSuccessModalElement extends UmbModalBaseElement<
	UmbCreateUserSuccessModalData,
	UmbCreateUserSuccessModalValue
> {
	@state()
	_userItem?: UmbUserItemModel;

	@state()
	_initialPassword: string = 'INITIAL PASSWORD GOES HERE';

	#userItemRepository = new UmbUserItemRepository(this);
	#notificationContext?: UmbNotificationContext;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => (this.#notificationContext = instance));
	}

	protected async firstUpdated(): Promise<void> {
		if (!this.data?.user.unique) throw new Error('No user unique is provided');

		// TODO: generate a new random password for the user, when the end point is ready
		const { data } = await this.#userItemRepository.requestItems([this.data?.user.unique]);
		if (data) {
			this._userItem = data[0];
		}
	}

	#copyPassword() {
		const passwordInput = this.shadowRoot?.querySelector('#password') as UUIInputPasswordElement;
		if (!passwordInput || typeof passwordInput.value !== 'string') return;

		navigator.clipboard.writeText(passwordInput.value);
		const data: UmbNotificationDefaultData = { message: 'Password copied' };
		this.#notificationContext?.peek('positive', { data });
	}

	#onCloseModal(event: Event) {
		event.stopPropagation();
		this.modalContext?.reject();
	}

	#onCreateAnotherUser(event: Event) {
		event.stopPropagation();
		this.modalContext?.reject({ type: 'createAnotherUser' });
	}

	#onGoToProfile(event: Event) {
		event.stopPropagation();
		history.pushState(null, '', 'section/user-management/view/users/user/' + this.data?.user.unique); //TODO: URL Should be dynamic
		this._submitModal();
	}

	render() {
		return html`<uui-dialog-layout headline="${this._userItem?.name} has been created">
			<p>The new user has successfully been created. To log in to Umbraco use the password below</p>
			<uui-form-layout-item>
				<uui-label slot="label" for="password">Password</uui-label>
				<div id="password-control">
					<uui-input-password id="password" label="password" name="password" value="${this._initialPassword}" readonly>
					</uui-input-password>
					<uui-button compact label="Copy" @click=${this.#copyPassword} look="outline"></uui-button>
				</div>
			</uui-form-layout-item>

			<uui-button @click=${this.#onCloseModal} slot="actions" label="Close" look="secondary"></uui-button>
			<uui-button @click=${this.#onCreateAnotherUser} slot="actions" label="Create another user" look="secondary">
			</uui-button>
			<uui-button @click=${this.#onGoToProfile} slot="actions" label="Go to profile" look="primary" color="positive">
			</uui-button>
		</uui-dialog-layout>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			p {
				width: 580px;
			}

			#password {
				width: 100%;
			}

			#password-control {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbCreateUserSuccessModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-user-success-modal': UmbCreateUserSuccessModalElement;
	}
}
