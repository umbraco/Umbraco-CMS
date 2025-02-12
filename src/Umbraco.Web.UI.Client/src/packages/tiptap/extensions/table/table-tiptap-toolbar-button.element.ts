import { UmbTiptapToolbarButtonElement } from '../../components/toolbar/tiptap-toolbar-button.element.js';
import type { UmbTiptapToolbarTableExtensionApi, UmbTiptapToolbarTableMenuItem } from './table.tiptap-toolbar-api.js';
import { css, customElement, html, ifDefined, query, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-table-tiptap-toolbar-button')
export class UmbTiptapToolbarTableToolbarButtonElement extends UmbTiptapToolbarButtonElement {
	@query('#table-menu-popover')
	private _popover?: UUIPopoverContainerElement;

	override api?: UmbTiptapToolbarTableExtensionApi;

	#onClick(item: UmbTiptapToolbarTableMenuItem) {
		if (!item.execute || !this.editor) return;

		item.execute(this.editor);

		setTimeout(() => {
			// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this._popover?.hidePopover();
		}, 100);
	}

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
			</uui-button>

			${when(
				this.api?.getMenu(),
				(menu) => html`
					<uui-popover-container id="table-menu-popover" placement="bottom-start">
						<umb-popover-layout>
							<uui-scroll-container>
								${repeat(
									menu,
									(item) => item.label,
									(item) => html`
										<uui-menu-item label=${item.label} @click-label=${() => this.#onClick(item)}>
											${when(item.icon, (icon) => html`<umb-icon slot="icon" name=${icon}></umb-icon>`)}
										</uui-menu-item>
									`,
								)}
							</uui-scroll-container>
						</umb-popover-layout>
					</uui-popover-container>
				`,
			)}
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-menu-item-flat-structure: 1;
			}

			uui-scroll-container {
				max-height: 500px;
			}
		`,
	];
}

export { UmbTiptapToolbarTableToolbarButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-table-tiptap-toolbar-button': UmbTiptapToolbarTableToolbarButtonElement;
	}
}
