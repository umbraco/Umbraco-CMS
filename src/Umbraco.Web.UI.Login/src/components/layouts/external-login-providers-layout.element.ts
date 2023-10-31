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
			${this.showDivider
				? html`
						<div id="divider" aria-hidden="true">
							<span><umb-localize key="general_or">Or</umb-localize></span>
						</div>
				  `
				: nothing}
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
				overflow: hidden;
			}

			#divider span {
				/* background-color: var(--uui-color-surface); */
				padding: 0 9px;
				text-transform: capitalize;
				overflow: hidden;
				position: relative;
			}

			#divider span::before,
			#divider span::after {
				content: '';
				display: block;
				width: 200px; /* Arbitrary value, just be bigger than 50% of the max width of the container */
				height: 1px;
				position: absolute;
				transform: translateY(-50%);
				top: calc(50% + 1px);
				z-index: -1;
				background-color: var(--uui-color-border);
			}
			#divider span::before {
				right: 100%;
			}
			#divider span::after {
				left: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-external-login-providers-layout': UmbExternalLoginProvidersLayoutElement;
	}
}
