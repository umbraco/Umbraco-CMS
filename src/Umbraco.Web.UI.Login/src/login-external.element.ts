import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui';

@customElement('umb-login-external')
export class UmbLoginExternalElement extends LitElement {
	@property({ attribute: 'custom-view' })
	customView?: any;

	@property({ attribute: 'custom-view' })
	options!: {
		name: string;
		buttonLook: InterfaceLook;
		buttonColor: InterfaceColor;
		icon: string;
	};

	render() {
		return html`
			<uui-icon-registry-essential>
				<uui-button compact look=${this.options.buttonLook} color=${this.options.buttonColor}>
					<div><uui-icon name=${this.options.icon}></uui-icon> Continue with ${this.options.name}</div>
				</uui-button>
			</uui-icon-registry-essential>
		`;
	}

	static styles: CSSResultGroup = [
		css`
			uui-button {
				width: 100%;
				--uui-button-padding-top-factor: 1.5;
				--uui-button-padding-bottom-factor: 1.5;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-login-external': UmbLoginExternalElement;
	}
}
