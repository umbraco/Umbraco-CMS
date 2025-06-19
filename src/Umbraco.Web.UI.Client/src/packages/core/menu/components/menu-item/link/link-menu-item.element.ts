import type { UmbMenuItemElement } from '../../../menu-item-element.interface.js';
import type { ManifestMenuItemLinkKind } from './types.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-link-menu-item';
@customElement(elementName)
export class UmbLinkMenuItemElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestMenuItemLinkKind;

	#getTarget() {
		const href = this.manifest?.meta.href;

		if (href && href.startsWith('http')) {
			return '_blank';
		}

		return '_self';
	}

	override render() {
		return html`
			<umb-menu-item-layout
				.href=${this.manifest?.meta.href}
				target=${this.#getTarget()}
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
