import { css, customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbMenuElement } from '@umbraco-cms/backoffice/menu';
import type { ManifestMenuItem } from '@umbraco-cms/backoffice/menu';

@customElement('umb-tiptap-menu')
export class UmbTiptapMenuElement extends UmbMenuElement {
	@property()
	menuAlias?: string;

	override render() {
		return html`
			${when(
				this.menuAlias || this.manifest?.alias,
				(menuAlias) => html`
					<umb-extension-with-api-slot
						type="menuItem"
						default-element="umb-action-menu-item"
						.filter=${(items: ManifestMenuItem) => items.meta.menus.includes(menuAlias)}></umb-extension-with-api-slot>
				`,
			)}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;

				display: flex;
				flex-direction: column;

				background-color: var(--uui-color-surface);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
			}
		`,
	];
}

export default UmbTiptapMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-menu': UmbTiptapMenuElement;
	}
}
