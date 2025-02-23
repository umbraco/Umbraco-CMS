import { UmbTiptapToolbarButtonElement } from '../../components/toolbar/tiptap-toolbar-button.element.js';
import type { UmbCascadingMenuItem } from '../../components/cascading-menu-popover/cascading-menu-popover.element.js';
import { css, customElement, html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

import '../../components/cascading-menu-popover/cascading-menu-popover.element.js';

@customElement('umb-tiptap-font-size-toolbar-element')
export class UmbTiptapToolbarFontSizeToolbarElement extends UmbTiptapToolbarButtonElement {
	#fontSizes = [8, 10, 12, 14, 16, 18, 24, 36, 48];

	#menu: Array<UmbCascadingMenuItem> = this.#fontSizes.map((fontSize) => ({
		unique: `font-size-${fontSize}pt`,
		label: `${fontSize}pt`,
		execute: () =>
			this.editor
				?.chain()
				.focus()
				.setMark('textStyle', { style: `font-size: ${fontSize}pt;` })
				.run(),
	}));

	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		return html`
			<uui-button compact look="secondary" label=${ifDefined(label)} popovertarget="font-size">
				<span>${label}</span>
				<uui-symbol-expand slot="extra" open></uui-symbol-expand>
			</uui-button>
			<umb-cascading-menu-popover id="font-size" placement="bottom-start" .items=${this.#menu}>
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

export { UmbTiptapToolbarFontSizeToolbarElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-font-size-toolbar-element': UmbTiptapToolbarFontSizeToolbarElement;
	}
}
