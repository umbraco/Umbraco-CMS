import { UmbUserItemRepository } from '../../repository/item/user-item.repository.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputPasswordElement } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbNotificationDefaultData,
	UmbNotificationContext,
	UMB_NOTIFICATION_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/notification';
import {
	UmbCreateUserSuccessModalData,
	UmbCreateUserSuccessModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';
import { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-user-create-success-modal')
export class UmbUserCreateSuccessModalElement extends UmbModalBaseElement<
	UmbCreateUserSuccessModalData,
	UmbCreateUserSuccessModalValue
> {
	@state()
	_userItem?: UserItemResponseModel;

	#userItemRepository = new UmbUserItemRepository(this);
	#notificationContext?: UmbNotificationContext;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => (this.#notificationContext = instance));
	}

	protected async firstUpdated(): Promise<void> {
		if (!this.data?.userId) throw new Error('No userId provided');

		const { data } = await this.#userItemRepository.requestItems([this.data?.userId]);
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
		history.pushState(null, '', 'section/user-management/view/users/user/' + this.id); //TODO: URL Should be dynamic
		this.modalContext?.submit();
	}

	render() {
		return html`<uui-dialog-layout headline="${this._userItem?.name} has been created">
			<p>The new user has successfully been created. To log in to Umbraco use the password below</p>
			<uui-form-layout-item>
				<uui-label slot="label" for="password">Password</uui-label>
				<div id="password-control">
					<uui-input-password
						id="password"
						label="password"
						name="password"
						value="${this.data?.initialPassword ?? ''}"
						readonly>
					</uui-input-password>
					<uui-button compact label="Copy" @click=${this.#copyPassword} look="outline"></uui-button>
				</div>
			</uui-form-layout-item>

			<uui-button @click=${this.#onCloseModal} slot="actions" label="Close" look="secondary"></uui-button>
			<uui-button
				@click=${this.#onCreateAnotherUser}
				slot="actions"
				label="Create another user"
				look="secondary">
			</uui-button>
			<uui-button
				@click=${this.#onGoToProfile}
				slot="actions"
				label="Go to profile"
				look="primary"
				color="positive">
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

export default UmbUserCreateSuccessModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-create-success-modal': UmbUserCreateSuccessModalElement;
	}
}
