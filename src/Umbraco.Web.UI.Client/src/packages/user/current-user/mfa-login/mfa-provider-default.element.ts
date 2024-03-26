import type { UmbMfaProviderConfigurationElementProps } from '../types.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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

	render() {
		return html`
			<umb-body-layout headline=${this.providerName}>
				<div slot="actions">
					<uui-button look="secondary" .label=${this.localize.term('general_close')} @click=${this.onClose}>
						${this.localize.term('general_close')}
					</uui-button>
					<uui-button look="primary" .label=${this.localize.term('general_submit')} @click=${this.#onSubmit}>
						${this.localize.term('general_submit')}
					</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#onSubmit() {
		this.onSubmit({ code: '123456', secret: '123' });
	}
}

export default UmbMfaProviderDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-mfa-provider-default': UmbMfaProviderDefaultElement;
	}
}
