import { html, ifDefined, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { ManifestMenuItem, UmbMenuItemExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-menu-item')
export class UmbMenuItemElement extends UmbLitElement implements UmbMenuItemExtensionElement {
	@property({ type: Object, attribute: false })
	manifest!: ManifestMenuItem;

	render() {
		return html`<umb-menu-item-base
			label=${this.manifest.meta.label || this.manifest.name}
			icon-name=${this.manifest.meta.icon}
			entity-type=${ifDefined(this.manifest.meta.entityType)}></umb-menu-item-base>`;
	}

	static styles = [UUITextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item': UmbMenuItemElement;
	}
}
