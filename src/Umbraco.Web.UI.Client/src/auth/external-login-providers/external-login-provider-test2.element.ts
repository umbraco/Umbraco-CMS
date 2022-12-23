import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';

@customElement('umb-external-login-provider-test2')
export class UmbExternalLoginProviderTest2Element extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
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
			uui-input {
				width: 100%;
			}
		`,
	];

	render() {
		return html`
			<b>Another Custom External Login Provider</b>
			<p>This is an example of another custom external login provider</p>
			<uui-form-layout-item>
				<uui-label id="emailLabel" for="email" slot="label" required>Email</uui-label>
				<uui-input
					type="email"
					id="email"
					name="email"
					placeholder="Enter your email..."
					required
					required-message="Email is required"></uui-input>
			</uui-form-layout-item>
			<uui-button label="Custom login" look="primary"></uui-button>
		`;
	}
}

export default UmbExternalLoginProviderTest2Element;

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-provider-test2': UmbExternalLoginProviderTest2Element;
	}
}
