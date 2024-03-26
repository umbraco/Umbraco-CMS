import type { UmbMfaProviderConfigurationElementProps } from '../../types.js';
import { UserResource } from '@umbraco-cms/backoffice/external/backend-api';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-mfa-provider-default')
export class UmbMfaProviderDefaultElement extends UmbLitElement implements UmbMfaProviderConfigurationElementProps {
	@property({ attribute: false })
	providerName = '';

	@property({ type: Boolean, attribute: false })
	isEnabled = false;

	@property({ attribute: false })
	onSubmit: (value: { code: string; secret?: string | undefined }) => void = () => {};

	@property({ attribute: false })
	onClose = () => {};

	@state()
	_loading = true;

	@state()
	_secret = '';

	@state()
	_qrCodeSetupImageUrl = '';

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});
	}

	async firstUpdated() {
		await this.#load();
		this._loading = false;
	}

	async #load() {
		if (!this.providerName) {
			this.#peek('Provider name is required');
			throw new Error('Provider name is required');
		}
		const { data } = await tryExecuteAndNotify(
			this,
			UserResource.getUserCurrent2FaByProviderName({ providerName: this.providerName }),
		);

		if (!data) {
			this.#peek('No data returned');
			throw new Error('No data returned');
		}

		// Verify that there is a secret
		if (!data.secret) {
			this.#peek('The provider did not return a secret.');
			throw new Error('No secret returned');
		}

		this._secret = data.secret;
		this._qrCodeSetupImageUrl = data.qrCodeSetupImageUrl;
	}

	render() {
		if (this._loading) {
			return html`<uui-loader-bar></uui-loader-bar>`;
		}

		return html`
			<form id="authForm" name="authForm" @submit=${this.#onSubmit}>
				<umb-body-layout headline=${this.providerName}>
					<div id="main"></div>
					<div slot="actions">
						<uui-button
							type="button"
							look="secondary"
							.label=${this.localize.term('general_close')}
							@click=${this.onClose}>
							${this.localize.term('general_close')}
						</uui-button>
						<uui-button type="submit" look="primary" .label=${this.localize.term('buttons_save')}>
							${this.localize.term('general_submit')}
						</uui-button>
					</div>
				</umb-body-layout>
			</form>
		`;
	}

	#peek(message: string) {
		this.#notificationContext?.peek('danger', {
			data: {
				headline: this.localize.term('member_2fa'),
				message,
			},
		});
	}

	#onSubmit(e: SubmitEvent) {
		e.preventDefault();
		this.onSubmit({ code: '123456', secret: '123' });
	}

	static styles = [
		UmbTextStyles,
		css`
			#authForm {
				height: 100%;
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
