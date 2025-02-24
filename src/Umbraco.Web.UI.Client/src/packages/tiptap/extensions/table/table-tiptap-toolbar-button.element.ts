import { UmbTiptapToolbarButtonElement } from '../../components/toolbar/tiptap-toolbar-button.element.js';
import type { UmbTiptapToolbarTableExtensionApi } from './table.tiptap-toolbar-api.js';
import { css, customElement, html, ifDefined, when } from '@umbraco-cms/backoffice/external/lit';

import '../../components/cascading-menu-popover/cascading-menu-popover.element.js';

@customElement('umb-tiptap-toolbar-table-button')
export class UmbTiptapToolbarTableButtonElement extends UmbTiptapToolbarButtonElement {
	override api?: UmbTiptapToolbarTableExtensionApi;

	override render() {
		return html`
			<uui-button
				compact
				look=${this.isActive ? 'outline' : 'default'}
				label=${ifDefined(this.manifest?.meta.label)}
				popovertarget="table-menu-popover"
				title=${this.manifest?.meta.label ? this.localize.string(this.manifest.meta.label) : ''}>
				${when(
					this.manifest?.meta.icon,
					(icon) => html`<umb-icon name=${icon}></umb-icon>`,
					() => html`<span>${this.manifest?.meta.label}</span>`,
				)}
				<uui-symbol-expand slot="extra" open></uui-symbol-expand>
			</uui-button>
			${when(
				this.api?.getMenu(this.editor),
				(menu) => html`
					<umb-cascading-menu-popover id="table-menu-popover" placement="bottom-start" .items=${menu}>
					</umb-cascading-menu-popover>
				`,
			)}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			uui-button > uui-symbol-expand {
				margin-left: var(--uui-size-space-1);
			}
		`,
	];
}

export { UmbTiptapToolbarTableButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-table-button': UmbTiptapToolbarTableButtonElement;
	}
}
