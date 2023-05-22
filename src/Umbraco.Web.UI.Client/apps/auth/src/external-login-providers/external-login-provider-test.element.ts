import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement } from 'lit/decorators.js';

@customElement('umb-external-login-provider-test')
export class UmbExternalLoginProviderTestElement extends LitElement {
	render() {
		return html`
			<b>Custom External Login Provider</b>
			<p>This is an example of a custom external login provider using the external login provider extension point</p>
			<uui-button label="My custom login provider" look="primary"></uui-button>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
				padding: var(--uui-size-space-5);
				border: 1px solid var(--uui-color-border);
				background: var(--uui-color-surface-alt);
				border-radius: var(--uui-border-radius);
			}
			p {
				margin: 0;
			}
		`,
	];
}

export default UmbExternalLoginProviderTestElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-provider-test': UmbExternalLoginProviderTestElement;
	}
}
