import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestMenuItemLinkKind, UmbMenuItemElement } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-link-menu-item';
@customElement(elementName)
export class UmbLinkMenuItemElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestMenuItemLinkKind;

	override render() {
		return html`
			<umb-menu-item-layout
				.href=${this.manifest?.meta.href}
				.iconName=${this.manifest?.meta.icon ?? ''}
				.label=${this.localize.string(this.manifest?.meta.label ?? this.manifest?.name ?? '')}>
			</umb-menu-item-layout>
		`;
	}
}

export { UmbLinkMenuItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbLinkMenuItemElement;
	}
}
