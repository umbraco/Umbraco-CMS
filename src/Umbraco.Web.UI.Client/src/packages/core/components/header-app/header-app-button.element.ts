import { css, customElement, html, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestHeaderAppButtonKind, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const manifest: UmbExtensionManifestKind = {
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
export class UmbHeaderAppButtonElement extends UmbLitElement {
	public manifest?: ManifestHeaderAppButtonKind;

	override render() {
		return html`
			<uui-button
				look="primary"
				label=${ifDefined(this.manifest?.meta.label)}
				href=${ifDefined(this.manifest?.meta.href)}
				compact>
				<umb-icon name=${ifDefined(this.manifest?.meta.icon)}></umb-icon>
			</uui-button>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-button {
				font-size: 18px;
				--uui-button-background-color: var(--umb-header-app-button-background-color, transparent);
				--uui-button-background-color-hover: var(
					--umb-header-app-button-background-color-hover,
					var(--uui-color-emphasis)
				);
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
