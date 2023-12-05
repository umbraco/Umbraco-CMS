import { UmbUserItemRepository } from '../../user/repository/item/user-item.repository.js';
import { UMB_CURRENT_USER_CONTEXT } from '../../current-user/current-user.context.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, CSSResultGroup, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbChangePasswordModalData,
	UmbChangePasswordModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';

@customElement('umb-change-password-modal')
export class UmbChangePasswordModalElement extends UmbModalBaseElement<
	UmbChangePasswordModalData,
	UmbChangePasswordModalValue
> {
	@state()
	private _headline: string = 'Change password';

	@state()
	private _isCurrentUser: boolean = false;

	#userItemRepository = new UmbUserItemRepository(this);
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	#onClose() {
		this.modalContext?.reject();
	}

	#onSubmit(e: SubmitEvent) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		// TODO: validate that the new password and confirm password match
		const oldPassword = formData.get('oldPassword') as string;
		const newPassword = formData.get('newPassword') as string;
		//const confirmPassword = formData.get('confirmPassword') as string;

		this._value = { oldPassword, newPassword };
		this.modalContext?.submit();
	}

	constructor() {
		super();

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
			this.#setIsCurrentUser();
		});
	}

	async #setIsCurrentUser() {
		if (!this.data?.userId) {
			this._isCurrentUser = false;
			return;
		}

		if (!this.#currentUserContext) {
			this._isCurrentUser = false;
			return;
		}

		this._isCurrentUser = await this.#currentUserContext.isUserCurrentUser(this.data.userId);
	}

	protected async firstUpdated(): Promise<void> {
		if (!this.data?.userId) return;
		const { data } = await this.#userItemRepository.requestItems([this.data.userId]);

		if (data) {
			const userName = data[0].name;
			this._headline = `Change password for ${userName}`;
		}
	}

	render() {
		return html`
			<uui-dialog-layout class="uui-text" headline=${this._headline}>
				<uui-form>
					<form id="ChangePasswordForm" @submit="${this.#onSubmit}">
						${this._isCurrentUser ? this.#renderOldPasswordInput() : nothing}
						<uui-form-layout-item>
							<uui-label id="newPasswordLabel" for="newPassword" slot="label" required>New password</uui-label>
							<uui-input-password
								id="newPassword"
								name="newPassword"
								required
								required-message="New password is required"></uui-input-password>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label id="confirmPasswordLabel" for="confirmPassword" slot="label" required
								>Confirm password</uui-label
							>
							<uui-input-password
								id="confirmPassword"
								name="confirmPassword"
								required
								required-message="Confirm password is required"></uui-input-password>
						</uui-form-layout-item>
					</form>
				</uui-form>

				<uui-button slot="actions" @click=${this.#onClose} label="Cancel"></uui-button>
				<uui-button
					slot="actions"
					type="submit"
					label="Confirm"
					look="primary"
					color="positive"
					form="ChangePasswordForm"></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderOldPasswordInput() {
		return html`
			<uui-form-layout-item style="margin-bottom: var(--uui-size-layout-2)">
				<uui-label id="oldPasswordLabel" for="oldPassword" slot="label" required>Old password</uui-label>
				<uui-input-password
					id="oldPassword"
					name="oldPassword"
					required
					required-message="Old password is required"></uui-input-password>
			</uui-form-layout-item>
		`;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			uui-input-password {
				width: 100%;
			}
		`,
	];
}

export default UmbChangePasswordModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-change-password-modal': UmbChangePasswordModalElement;
	}
}
