import type { ManifestMenuItemActionKind, UmbMenuItemActionApi, UmbMenuItemActionElement } from './types.js';
import { customElement, html, ifDefined, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIMenuItemEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-action-menu-item')
export default class UmbActionMenuItemElement extends UmbLitElement implements UmbMenuItemActionElement {
	@property({ attribute: false })
	api?: UmbMenuItemActionApi;

	@property({ attribute: false })
	manifest?: ManifestMenuItemActionKind;

	#onClickLabel(event: UUIMenuItemEvent) {
		event.stopPropagation();

		try {
			this.api?.execute();
		} catch (error) {
			console.error('Error menu item action:', error);
		}
	}

	// Prevents the regular click event from bubbling up.
	// This could be handled in the UUI Menu item component, to prevent dispatching both "click-label" and "click" events at the same time.
	#onClick(event: PointerEvent) {
		event.stopPropagation();
	}

	override render() {
		const label = this.localize.string(this.manifest?.meta.label ?? this.manifest?.name);
		return html`
			<uui-menu-item label=${ifDefined(label)} @click-label=${this.#onClickLabel} @click=${this.#onClick}>
				${when(this.manifest?.meta.icon, (icon) => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
			</uui-menu-item>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-action-menu-item': UmbActionMenuItemElement;
	}
}
