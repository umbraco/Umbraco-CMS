import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, queryAssignedElements } from 'lit/decorators.js';

@customElement('umb-external-login-providers-layout')
export class UmbExternalLoginProvidersLayoutElement extends LitElement {
	@queryAssignedElements({ flatten: true })
	protected slottedElements?: HTMLElement[];

	firstUpdated() {
		!!this.slottedElements?.length ? this.toggleAttribute('empty', false) : this.toggleAttribute('empty', true);
	}

	render() {
		return html`
			<div id="divider"><span>or</span></div>
			<div>
				<slot></slot>
			</div>
		`;
	}

	static styles: CSSResultGroup = [
		css`
			:host {
				margin-top: 16px;
				display: flex;
				flex-direction: column;
			}

			:host([empty]) {
				display: none;
			}

			slot {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-4);
			}

			#divider {
				width: 100%;
				text-align: center;
				color: #868686; /* TODO: Change to uui color when uui gets a muted text variable */
				position: relative;
				z-index: 0;
				margin-bottom: 16px;
			}

			#divider span {
				background-color: var(--uui-color-surface-alt);
				padding: 0 9px;
			}

			#divider::before {
				content: '';
				display: block;
				width: 100%;
				height: 1px;
				background-color: var(--uui-color-border);
				position: absolute;
				top: calc(50% + 1px);
				transform: translateY(-50%);
				z-index: -1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-providers-layout': UmbExternalLoginProvidersLayoutElement;
	}
}
