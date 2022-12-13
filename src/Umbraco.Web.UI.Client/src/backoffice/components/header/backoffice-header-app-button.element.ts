import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-backoffice-header-app-button')
export class UmbBackofficeHeaderAppButton extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 18px;
			}
		`,
	];

	public extensionMeta: any = {};

	render() {
		return html`
			<uui-button look="primary" label="${this.extensionMeta.label}" compact>
				<uui-icon name="${this.extensionMeta.icon}"></uui-icon>
			</uui-button>
		`;
	}
}

export default UmbBackofficeHeaderAppButton;

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-app-button': UmbBackofficeHeaderAppButton;
	}
}
