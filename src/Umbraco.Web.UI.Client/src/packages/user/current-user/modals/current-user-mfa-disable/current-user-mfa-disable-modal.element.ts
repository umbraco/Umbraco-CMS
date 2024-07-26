import { UmbCurrentUserRepository } from '../../repository/index.js';
import type { UmbCurrentUserMfaDisableModalConfig } from './current-user-mfa-disable-modal.token.js';
import { UMB_NOTIFICATION_CONTEXT, type UmbNotificationColor } from '@umbraco-cms/backoffice/notification';
import { css, customElement, html, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { isApiError } from '@umbraco-cms/backoffice/resources';

@customElement('umb-current-user-mfa-disable-modal')
export class UmbCurrentUserMfaDisableModalElement extends UmbModalBaseElement<
	UmbCurrentUserMfaDisableModalConfig,
	never
> {
	#currentUserRepository = new UmbCurrentUserRepository(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	@state()
	_buttonState?: UUIButtonState;

	@query('#code')
	_codeInput!: HTMLInputElement;

	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	override render() {
		if (!this.data) {
			return html`<uui-loader-bar></uui-loader-bar>`;
		}

		return html`
			<uui-form>
				<form id="authForm" name="authForm" @submit=${this.#onSubmit} novalidate>
					<umb-body-layout headline=${this.data.displayName}>
						<div id="main">
							<p>
								<umb-localize key="user_2faDisableText">
									If you wish to disable this two-factor provider, then you must enter the code shown on your
									authentication device:
								</umb-localize>
							</p>

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
						</div>
						<div slot="actions">
							<uui-button
								type="button"
								look="secondary"
								.label=${this.localize.term('general_close')}
								@click=${this.#close}>
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

	async #onSubmit(e: SubmitEvent) {
		e.preventDefault();

		this._codeInput.setCustomValidity('');

		if (!this.data) throw new Error('No data provided');

		const form = e.target as HTMLFormElement;

		if (!form.checkValidity()) return;

		const formData = new FormData(form);
		const code = formData.get('code') as string;

		if (!code) return;

		this._buttonState = 'waiting';
		const { error } = await this.#currentUserRepository.disableMfaProvider(this.data.providerName, code);

		if (!error) {
			this.#peek(this.localize.term('user_2faProviderIsDisabledMsg', this.data.displayName ?? this.data.providerName));
			this.modalContext?.submit();
			this._buttonState = 'success';
		} else {
			this._buttonState = 'failed';
			if (isApiError(error)) {
				if ((error.body as any).operationStatus === 'InvalidCode') {
					this._codeInput.setCustomValidity(this.localize.term('user_2faInvalidCode'));
					this._codeInput.focus();
				} else {
					this.#peek(
						this.localize.term('user_2faProviderIsNotDisabledMsg', this.data.displayName ?? this.data.providerName),
						'warning',
					);
				}
			} else {
				this.#peek(error.message, 'warning');
			}
		}
	}

	async #close() {
		this.modalContext?.submit();
	}

	async #peek(message: string, color?: UmbNotificationColor) {
		this.#notificationContext?.peek(color ?? 'positive', {
			data: {
				headline: this.localize.term('member_2fa'),
				message,
			},
		});
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			#authForm {
				height: 100%;
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

export default UmbCurrentUserMfaDisableModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-current-user-mfa-disable-modal': UmbCurrentUserMfaDisableModalElement;
	}
}
