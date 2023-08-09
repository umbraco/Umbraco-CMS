import { UUITextStyles } from '@umbraco-ui/uui-css';
import { CSSResultGroup, LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-error-layout')
export default class UmbErrorLayoutElement extends LitElement {
	@property({ type: String })
	header = '';

	@property({ type: String })
	message = '';

	render() {
		return html`
			<div id="header">
				<h2>${this.header}</h2>
				<span>${this.message}</span>
			</div>
			<slot></slot>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-error-layout': UmbErrorLayoutElement;
	}
}
