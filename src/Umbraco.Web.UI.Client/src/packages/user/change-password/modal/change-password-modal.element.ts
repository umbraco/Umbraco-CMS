import type { UmbChangePasswordModalData, UmbChangePasswordModalValue } from './change-password-modal.token.js';
import { css, customElement, html, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbUserItemRepository } from '@umbraco-cms/backoffice/user';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UUIInputPasswordElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-change-password-modal')
export class UmbChangePasswordModalElement extends UmbModalBaseElement<
	UmbChangePasswordModalData,
	UmbChangePasswordModalValue
> {
	@query('#newPassword')
	private _newPasswordInput?: UUIInputPasswordElement;

	@query('#confirmPassword')
	private _confirmPasswordInput?: UUIInputPasswordElement;

	@state()
	private _headline: string = this.localize.term('general_changePassword');

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

		const oldPassword = formData.get('oldPassword') as string;
		const newPassword = formData.get('newPassword') as string;

		this.value = { oldPassword, newPassword };
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
		if (!this.#currentUserContext || !this.data?.user.unique) {
			this._isCurrentUser = false;
			return;
		}

		this._isCurrentUser = await this.#currentUserContext.isUserCurrentUser(this.data.user.unique);
	}

	protected override async firstUpdated() {
		this._confirmPasswordInput?.addValidator(
			'customError',
			() => this.localize.term('user_passwordMismatch'),
			() => this._confirmPasswordInput?.value !== this._newPasswordInput?.value,
		);

		if (!this.data?.user.unique) return;
		const { data } = await this.#userItemRepository.requestItems([this.data.user.unique]);

		if (data) {
			const userName = data[0].name;
			this._headline = `Change password for ${userName}`;
		}
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" headline=${this._headline}>
				<uui-form>
					<form id="ChangePasswordForm" @submit=${this.#onSubmit}>
						${when(
							this._isCurrentUser,
							() => html`
								<uui-form-layout-item style="margin-bottom: var(--uui-size-layout-2)">
									<uui-label slot="label" id="oldPasswordLabel" for="oldPassword" required>
										<umb-localize key="user_passwordCurrent">Current password</umb-localize>
									</uui-label>
									<uui-input-password
										id="oldPassword"
										name="oldPassword"
										required
										required-message="Current password is required">
									</uui-input-password>
								</uui-form-layout-item>
							`,
						)}
						<uui-form-layout-item>
							<uui-label slot="label" id="newPasswordLabel" for="newPassword" required>
								<umb-localize key="user_newPassword">New password</umb-localize>
							</uui-label>
							<uui-input-password
								id="newPassword"
								name="newPassword"
								required
								required-message="New password is required">
							</uui-input-password>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label slot="label" id="confirmPasswordLabel" for="confirmPassword" required>
								<umb-localize key="user_confirmNewPassword">Confirm new password</umb-localize>
							</uui-label>
							<uui-input-password
								id="confirmPassword"
								name="confirmPassword"
								required
								required-message="Confirm password is required">
							</uui-input-password>
						</uui-form-layout-item>
					</form>
				</uui-form>
				<uui-button slot="actions" label=${this.localize.term('general_cancel')} @click=${this.#onClose}></uui-button>
				<uui-button
					slot="actions"
					type="submit"
					form="ChangePasswordForm"
					color="positive"
					look="primary"
					label=${this.localize.term('general_confirm')}></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override readonly styles = [
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
