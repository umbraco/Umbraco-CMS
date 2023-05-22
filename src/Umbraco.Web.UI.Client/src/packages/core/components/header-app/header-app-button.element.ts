import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import {
	ManifestHeaderAppButtonKind,
	UmbBackofficeManifestKind,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';

const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Button',
	matchKind: 'button',
	matchType: 'headerApp',
	manifest: {
		type: 'headerApp',
		kind: 'button',
		elementName: 'umb-header-app-button',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-header-app-button')
export class UmbHeaderAppButtonElement extends LitElement {
	public manifest?: ManifestHeaderAppButtonKind;

	render() {
		return html`
			<uui-button
				look="primary"
				label="${ifDefined(this.manifest?.meta.label)}"
				href="${ifDefined(this.manifest?.meta.href)}"
				compact>
				<uui-icon name="${ifDefined(this.manifest?.meta.icon)}"></uui-icon>
			</uui-button>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 18px;
				--uui-button-background-color: transparent;
			}
		`,
	];
}

export default UmbHeaderAppButtonElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-header-app-button': UmbHeaderAppButtonElement;
	}
}
