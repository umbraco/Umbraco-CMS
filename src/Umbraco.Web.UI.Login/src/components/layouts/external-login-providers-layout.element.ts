import { css, CSSResultGroup, html, LitElement, nothing } from 'lit';
import { customElement, property, queryAssignedElements } from 'lit/decorators.js';

@customElement('umb-external-login-providers-layout')
export class UmbExternalLoginProvidersLayoutElement extends LitElement {
	@property({ type: Boolean, attribute: 'divider' })
	showDivider = true;

	@queryAssignedElements({ flatten: true })
	protected slottedElements?: HTMLElement[];

	firstUpdated() {
		!!this.slottedElements?.length ? this.toggleAttribute('empty', false) : this.toggleAttribute('empty', true);
	}

	render() {
		return html`
			${this.showDivider ? html` <div id="divider" aria-hidden="true"><span><umb-localize key="general_or">Or</umb-localize></span></div> ` : nothing}
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
				color: var(--uui-color-interactive);
				position: relative;
				z-index: 0;
				margin-bottom: 16px;
			}

			#divider span {
				background-color: var(--uui-color-surface-alt);
				padding: 0 9px;
        text-transform: capitalize;
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
