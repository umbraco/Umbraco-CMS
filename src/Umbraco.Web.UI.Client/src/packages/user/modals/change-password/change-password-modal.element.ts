import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, CSSResultGroup, html, nothing, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangePasswordModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-change-password-modal')
export class UmbChangePasswordModalElement extends UmbModalBaseElement<UmbChangePasswordModalData> {
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

		const oldPassword = formData.get('oldPassword') as string;
		const newPassword = formData.get('newPassword') as string;
		const confirmPassword = formData.get('confirmPassword') as string;

		this.modalContext?.submit({ oldPassword, newPassword, confirmPassword });
	}

	render() {
		return html`
			<uui-dialog-layout class="uui-text" headline="Change password">
				<uui-form>
					<form id="LoginForm" name="login" @submit="${this.#onSubmit}">
						${this.data?.requireOldPassword ? this.#renderOldPasswordInput() : nothing}
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

						<div id="actions">
							<uui-button @click=${this.#onClose} label="Cancel"></uui-button>
							<uui-button type="submit" label="Confirm" look="primary" color="positive"></uui-button>
						</div>
					</form>
				</uui-form>
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
			:host {
				display: block;
				width: 400px;
			}
			uui-input-password {
				width: 100%;
			}
			#actions {
				display: flex;
				justify-content: flex-end;
				margin-top: var(--uui-size-layout-2);
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
