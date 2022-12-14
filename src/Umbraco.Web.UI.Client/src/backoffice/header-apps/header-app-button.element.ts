import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { ManifestHeaderApp } from '@umbraco-cms/extensions-registry';

@customElement('umb-header-app-button')
export class UmbHeaderAppButton extends LitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 18px;
			}
		`,
	];

	public manifest?: ManifestHeaderApp;

	render() {
		return html`
			<uui-button look="primary" label="${ifDefined(this.manifest?.meta.label)}" compact>
				<uui-icon name="${ifDefined(this.manifest?.meta.icon)}"></uui-icon>
			</uui-button>
		`;
	}
}

export default UmbHeaderAppButton;

declare global {
	interface HTMLElementTagNameMap {
		'umb-header-app-button': UmbHeaderAppButton;
	}
}
