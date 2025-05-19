import { UmbTiptapToolbarMenuElement } from '../../../components/toolbar/tiptap-toolbar-menu.element.js';
import { customElement, html, ifDefined, when } from '@umbraco-cms/backoffice/external/lit';

import './table-insert.element.js';

@customElement('umb-tiptap-table-toolbar-menu-element')
export class UmbTiptapTableToolbarMenuElement extends UmbTiptapToolbarMenuElement {
	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		return html`
			${when(
				this.isActive,
				() => html`
					<uui-button compact look="outline" label=${ifDefined(label)} title=${label} popovertarget="popover-menu">
						${when(
							this.manifest?.meta.icon,
							(icon) => html`<umb-icon name=${icon}></umb-icon>`,
							() => html`<span>${label}</span>`,
						)}
						<uui-symbol-expand slot="extra" open></uui-symbol-expand>
					</uui-button>
				`,
				() => html`
					<uui-button compact look="default" label=${ifDefined(label)} title=${label} popovertarget="popover-insert">
						${when(
							this.manifest?.meta.icon,
							(icon) => html`<umb-icon name=${icon}></umb-icon>`,
							() => html`<span>${label}</span>`,
						)}
						<uui-symbol-expand slot="extra" open></uui-symbol-expand>
					</uui-button>
				`,
			)}
			${this.renderMenu()}
			<uui-popover-container id="popover-insert">
				<umb-tiptap-table-insert .editor=${this.editor}></umb-tiptap-table-insert>
			</uui-popover-container>
		`;
	}
}

export { UmbTiptapTableToolbarMenuElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-table-toolbar-menu-element': UmbTiptapTableToolbarMenuElement;
	}
}
