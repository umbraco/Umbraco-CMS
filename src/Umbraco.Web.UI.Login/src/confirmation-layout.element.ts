import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-confirmation-layout')
export default class UmbConfirmationLayoutElement extends LitElement {
	render() {
		return html`
			<div id="confirm-page">
				<div id="header">
					<h2><slot name="header"></slot></h2>
					<span><slot></slot></span>
				</div>

				<uui-button type="submit" label="Login" look="primary" color="default" href="login"></uui-button>
			</div>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#header {
				text-align: center;
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}
			#header span {
				color: #868686; /* TODO Change to uui color when uui gets a muted text variable */
				font-size: 14px;
			}
			#header h2 {
				margin: 0px;
				font-weight: bold;
				font-size: 1.4rem;
			}
			uui-button {
				width: 100%;
				margin-top: var(--uui-size-space-5);
				--uui-button-padding-top-factor: 1.5;
				--uui-button-padding-bottom-factor: 1.5;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-confirmation-layout': UmbConfirmationLayoutElement;
	}
}
