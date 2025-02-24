import { UmbTiptapToolbarButtonElement } from '../../components/toolbar/tiptap-toolbar-button.element.js';
import type { UmbCascadingMenuItem } from '../../components/cascading-menu-popover/cascading-menu-popover.element.js';
import { css, customElement, html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

import '../../components/cascading-menu-popover/cascading-menu-popover.element.js';

@customElement('umb-tiptap-font-family-toolbar-element')
export class UmbTiptapToolbarFontFamilyToolbarElement extends UmbTiptapToolbarButtonElement {
	#menu: Array<UmbCascadingMenuItem> = [
		{
			unique: 'font-family-sans-serif',
			label: 'Sans serif',
			element: this.#getElement('sans-serif', 'Sans serif'),
		},
		{
			unique: 'font-family-serif',
			label: 'Serif',
			element: this.#getElement('serif', 'Serif'),
		},
		{
			unique: 'font-family-monospace',
			label: 'Monospace',
			element: this.#getElement('monospace', 'Monospace'),
		},
		{
			unique: 'font-family-cursive',
			label: 'Cursive',
			element: this.#getElement('cursive', 'Cursive'),
		},
		{
			unique: 'font-family-fantasy',
			label: 'Fantasy',
			element: this.#getElement('fantasy', 'Fantasy'),
		},
	];

	#getElement(fontFamily: string, label: string) {
		const menuItem = document.createElement('uui-menu-item');
		menuItem.addEventListener('click', () => {
			//this.editor?.chain().focus().setMark('textStyle', { fontFamily }).run();
			this.editor?.chain().focus().setSpanStyle(`font-family: ${fontFamily};`).run();
		});

		const element = document.createElement('span');
		element.slot = 'label';
		element.textContent = label;
		element.style.fontFamily = fontFamily;

		menuItem.appendChild(element);

		return menuItem;
	}

	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		return html`
			<uui-button compact look="secondary" label=${ifDefined(label)} popovertarget="font-family">
				<span>${label}</span>
				<uui-symbol-expand slot="extra" open></uui-symbol-expand>
			</uui-button>
			<umb-cascading-menu-popover id="font-family" placement="bottom-start" .items=${this.#menu}>
			</umb-cascading-menu-popover>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-button-font-weight: normal;
				--uui-menu-item-flat-structure: 1;
			}

			uui-button > uui-symbol-expand {
				margin-left: var(--uui-size-space-4);
			}
		`,
	];
}

export { UmbTiptapToolbarFontFamilyToolbarElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-font-family-toolbar-element': UmbTiptapToolbarFontFamilyToolbarElement;
	}
}
