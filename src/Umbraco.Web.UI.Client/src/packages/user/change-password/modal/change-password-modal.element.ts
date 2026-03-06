import type { UmbChangePasswordModalData, UmbChangePasswordModalValue } from './change-password-modal.token.js';
import { css, customElement, html, query, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbUserItemRepository, UmbUserConfigRepository } from '@umbraco-cms/backoffice/user';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UUIInputPasswordElement } from '@umbraco-cms/backoffice/external/uui';
import type { PasswordConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

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

	@state()
	private _minimumPasswordLength = 10;

	@state()
	private _passwordConfiguration?: PasswordConfigurationResponseModel;

	@state()
	private _passwordPattern = '';

	@state()
	private _isLoadingConfig = true;

	#userItemRepository = new UmbUserItemRepository(this);
	#userConfigRepository = new UmbUserConfigRepository(this);
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#validationDebounceTimer?: ReturnType<typeof setTimeout>;

	#onClose() {
		this.modalContext?.reject();
	}

	#onPasswordInput = () => {
		// Clear any existing timer
		if (this.#validationDebounceTimer) {
			clearTimeout(this.#validationDebounceTimer);
		}

		// Set a new timer for 200ms
		this.#validationDebounceTimer = setTimeout(() => {
			if (!this._newPasswordInput) return;

			const password = this._newPasswordInput.value as string;
			if (!password) {
				// Clear validation for empty password
				this._newPasswordInput.setCustomValidity('');
				return;
			}

			// Validate the password
			const passwordError = this.#validatePassword(password);
			if (passwordError) {
				this._newPasswordInput.setCustomValidity(passwordError);
			} else {
				this._newPasswordInput.setCustomValidity('');
			}
		}, 200);
	};

	#validatePasswordLength(password: string): string | null {
		if (!this._passwordConfiguration) return null;
		if (this._passwordConfiguration.minimumPasswordLength > 0 && password.length < this._passwordConfiguration.minimumPasswordLength) {
			return this.localize.term('user_newPasswordFormatLengthTip', [this._passwordConfiguration.minimumPasswordLength]);
		}
		return null;
	}

	#validatePasswordDigit(password: string): string | null {
		if (!this._passwordConfiguration?.requireDigit) return null;
		const hasDigit = /\d/.test(password);
		if (!hasDigit) {
			return this.localize.term('user_passwordRequiresDigit');
		}
		return null;
	}

	#validatePasswordLowercase(password: string): string | null {
		if (!this._passwordConfiguration?.requireLowercase) return null;
		const hasLowercase = /[a-z]/.test(password);
		if (!hasLowercase) {
			return this.localize.term('user_passwordRequiresLower');
		}
		return null;
	}

	#validatePasswordUppercase(password: string): string | null {
		if (!this._passwordConfiguration?.requireUppercase) return null;
		const hasUppercase = /[A-Z]/.test(password);
		if (!hasUppercase) {
			return this.localize.term('user_passwordRequiresUpper');
		}
		return null;
	}

	#validatePasswordNonLetterOrDigit(password: string): string | null {
		if (!this._passwordConfiguration?.requireNonLetterOrDigit) return null;
		const hasNonLetterOrDigit = /[^a-zA-Z0-9]/.test(password);
		if (!hasNonLetterOrDigit) {
			return this.localize.term('user_passwordRequiresNonAlphanumeric');
		}
		return null;
	}

	#validatePassword(password: string): string | null {
		// Test against full pattern first
		if (this._passwordPattern) {
			const regex = new RegExp(this._passwordPattern);
			if (regex.test(password)) {
				return null; // Password is valid
			}
		}

		// If pattern doesn't match, check each requirement to provide specific feedback
		const lengthError = this.#validatePasswordLength(password);
		if (lengthError) return lengthError;

		const digitError = this.#validatePasswordDigit(password);
		if (digitError) return digitError;

		const lowercaseError = this.#validatePasswordLowercase(password);
		if (lowercaseError) return lowercaseError;

		const uppercaseError = this.#validatePasswordUppercase(password);
		if (uppercaseError) return uppercaseError;

		const specialCharError = this.#validatePasswordNonLetterOrDigit(password);
		if (specialCharError) return specialCharError;

		return null;
	}

	#onSubmit(e: SubmitEvent) {
		e.preventDefault();

		const form = e.target as HTMLFormElement;
		if (!form) return;

		// Prevent submission if configuration is still loading
		if (this._isLoadingConfig || !this._passwordConfiguration) {
			return;
		}

		// Re-validate to ensure validation state is current
		const passwordError = this.#validatePassword(this._newPasswordInput?.value as string);
		if (passwordError && this._newPasswordInput) {
			this._newPasswordInput.setCustomValidity(passwordError);
		} else if (this._newPasswordInput) {
			this._newPasswordInput.setCustomValidity('');
		}

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

		this.#loadPasswordConfiguration();
	}

	override disconnectedCallback() {
		super.disconnectedCallback();
		// Clean up debounce timer
		if (this.#validationDebounceTimer) {
			clearTimeout(this.#validationDebounceTimer);
		}
	}

	async #loadPasswordConfiguration() {
		await this.#userConfigRepository.initialized;
		this.observe(this.#userConfigRepository.part('passwordConfiguration'), (passwordConfig) => {
			this._passwordConfiguration = passwordConfig;
			this._minimumPasswordLength = passwordConfig.minimumPasswordLength ?? this._minimumPasswordLength;
			
			// Build password pattern
			let pattern = '';
			if (passwordConfig.requireDigit) {
				pattern += '(?=.*\\d)';
			}
			if (passwordConfig.requireLowercase) {
				pattern += '(?=.*[a-z])';
			}
			if (passwordConfig.requireUppercase) {
				pattern += '(?=.*[A-Z])';
			}
			if (passwordConfig.requireNonLetterOrDigit) {
				pattern += '(?=.*[^a-zA-Z0-9])';
			}
			pattern += `.{${passwordConfig.minimumPasswordLength ?? 10},}`;
			this._passwordPattern = pattern;
			this._isLoadingConfig = false;
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
								.minlength=${this._minimumPasswordLength}
								required
								required-message="New password is required"
								.minlengthMessage=${this.localize.term('user_newPasswordFormatLengthTip', [this._minimumPasswordLength])}
								@input=${this.#onPasswordInput}>
							</uui-input-password>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label slot="label" id="confirmPasswordLabel" for="confirmPassword" required>
								<umb-localize key="user_confirmNewPassword">Confirm new password</umb-localize>
							</uui-label>
							<uui-input-password
								id="confirmPassword"
								name="confirmPassword"
								.minlength=${this._minimumPasswordLength}
								required
								required-message="Confirm password is required"
								.minlengthMessage=${this.localize.term('user_newPasswordFormatLengthTip', [this._minimumPasswordLength])}>
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
					?disabled=${this._isLoadingConfig}
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
