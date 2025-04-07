import type { UmbMfaProviderConfigurationCallback, UmbMfaProviderConfigurationElementProps } from '../types.js';
import { UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { css, customElement, html, property, state, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecute, UmbApiError } from '@umbraco-cms/backoffice/resources';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationColor } from '@umbraco-cms/backoffice/notification';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

/**
 * A default MFA provider configuration element.
 * @element umb-mfa-provider-default
 */
@customElement('umb-mfa-provider-default')
export class UmbMfaProviderDefaultElement extends UmbLitElement implements UmbMfaProviderConfigurationElementProps {
	@property({ attribute: false })
	providerName = '';

	@property({ attribute: false })
	displayName = '';

	@property({ attribute: false })
	callback: (providerName: string, code: string, secret: string) => UmbMfaProviderConfigurationCallback =
		async () => ({});

	@property({ attribute: false })
	close = () => {};

	@state()
	protected _loading = true;

	@state()
	protected _secret = '';

	@state()
	protected _qrCodeSetupImageUrl = '';

	@state()
	protected _buttonState?: UUIButtonState;

	protected notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	@query('#code')
	protected codeField?: HTMLInputElement;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.notificationContext = context;
		});
	}

	protected override async firstUpdated() {
		await this.#load();
		this._loading = false;
	}

	async #load() {
		if (!this.providerName) {
			this.peek('Provider name is required', 'danger');
			throw new Error('Provider name is required');
		}
		const { data: _data } = await tryExecute(
			this,
			UserService.getUserCurrent2FaByProviderName({ providerName: this.providerName }),
		);
		const data: any = _data;
		if (!data) {
			this.peek('No data returned', 'danger');
			throw new Error('No data returned');
		}

		// Verify that there is a secret
		if (!data.secret) {
			this.peek('The provider did not return a secret.', 'danger');
			throw new Error('No secret returned');
		}

		this._secret = data.secret;
		this._qrCodeSetupImageUrl = data.qrCodeSetupImageUrl;
	}

	override render() {
		if (this._loading) {
			return html`<uui-loader-bar></uui-loader-bar>`;
		}

		return html`
			<uui-form>
				<form id="authForm" name="authForm" @submit=${this.submit} novalidate>
					<umb-body-layout headline=${this.displayName}>
						<div id="main">
							<uui-box .headline=${this.localize.term('member_2fa')}>
								<div class="text-center">
									<p>
										<umb-localize key="user_2faQrCodeDescription">
											Scan this QR code with your authenticator app to enable two-factor authentication
										</umb-localize>
									</p>
									<img
										id="qrCode"
										.src=${this._qrCodeSetupImageUrl}
										alt=${this.localize.term('user_2faQrCodeAlt')}
										title=${this.localize.term('user_2faQrCodeTitle')}
										loading="eager" />
								</div>
								<uui-form-layout-item class="text-center">
									<uui-label for="code" slot="label" required>
										<umb-localize key="user_2faCodeInput"></umb-localize>
									</uui-label>
									<uui-input
										id="code"
										name="code"
										type="text"
										inputmode="numeric"
										autocomplete="one-time-code"
										required
										required-message=${this.localize.term('general_required')}
										label=${this.localize.term('user_2faCodeInputHelp')}
										placeholder=${this.localize.term('user_2faCodeInputHelp')}></uui-input>
								</uui-form-layout-item>
							</uui-box>
						</div>
						<div slot="actions">
							<uui-button
								type="button"
								look="secondary"
								.label=${this.localize.term('general_close')}
								@click=${this.close}>
								${this.localize.term('general_close')}
							</uui-button>
							<uui-button
								.state=${this._buttonState}
								type="submit"
								look="primary"
								.label=${this.localize.term('buttons_save')}>
								${this.localize.term('general_submit')}
							</uui-button>
						</div>
					</umb-body-layout>
				</form>
			</uui-form>
		`;
	}

	/**
	 * Show a peek notification with a message.
	 * @param message The message to show.
	 * @param color
	 */
	protected peek(message: string, color?: UmbNotificationColor) {
		this.notificationContext?.peek(color ?? 'positive', {
			data: {
				headline: this.localize.term('member_2fa'),
				message,
			},
		});
	}

	/**
	 * Submit the form with the code and secret back to the opener.
	 * @param e The submit event
	 */
	protected async submit(e: SubmitEvent) {
		e.preventDefault();
		this.codeField?.setCustomValidity('');

		const form = e.target as HTMLFormElement;

		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const code = formData.get('code') as string;

		if (!code) return;

		this._buttonState = 'waiting';
		const { error } = await this.callback(this.providerName, code, this._secret);

		if (!error) {
			this.peek(this.localize.term('user_2faProviderIsEnabledMsg', this.displayName ?? this.providerName));
			this._buttonState = 'success';
			this.close();
		} else {
			this._buttonState = 'failed';
			if (UmbApiError.isUmbApiError(error)) {
				if (error.problemDetails.operationStatus === 'InvalidCode') {
					this.codeField?.setCustomValidity(this.localize.term('user_2faInvalidCode'));
					this.codeField?.focus();
				} else {
					this.peek(
						this.localize.term('user_2faProviderIsNotEnabledMsg', this.displayName ?? this.providerName),
						'warning',
					);
				}
			} else {
				this.peek(error.message, 'warning');
			}
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			#authForm {
				height: 100%;
			}

			#qrCode {
				width: 100%;
				aspect-ratio: 1;
			}

			#code {
				width: 100%;
				max-width: 300px;
			}

			.text-center {
				text-align: center;
			}
		`,
	];
}

export default UmbMfaProviderDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-mfa-provider-default': UmbMfaProviderDefaultElement;
	}
}
