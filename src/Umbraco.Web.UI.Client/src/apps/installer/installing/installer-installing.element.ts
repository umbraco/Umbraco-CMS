import { css, CSSResultGroup, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-installer-installing')
export class UmbInstallerInstallingElement extends LitElement {
	render() {
		return html` <div class="uui-text" data-test="installer-installing">
			<h1 class="uui-h3">Installing Umbraco</h1>
			<uui-loader-bar></uui-loader-bar>
		</div>`;
	}

	static styles: CSSResultGroup = [
		css`
			h1 {
				text-align: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-installing': UmbInstallerInstallingElement;
	}
}
