import type {
	ManifestTiptapToolbarExtensionMenuKind,
	MetaTiptapToolbarMenuItem,
	UmbTiptapToolbarElementApi,
} from '../../extensions/index.js';
import type { UmbCascadingMenuItem } from '../cascading-menu-popover/cascading-menu-popover.element.js';
import { css, customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { ManifestMenu } from '@umbraco-cms/backoffice/menu';

import '../cascading-menu-popover/cascading-menu-popover.element.js';

@customElement('umb-tiptap-toolbar-menu')
export class UmbTiptapToolbarMenuElement extends UmbLitElement {
	#menu: Array<UmbCascadingMenuItem> = [];

	@state()
	protected isActive = false;

	public api?: UmbTiptapToolbarElementApi;

	public editor?: Editor;

	public set manifest(value: ManifestTiptapToolbarExtensionMenuKind | undefined) {
		this.#manifest = value;
		this.#setMenu();
	}
	public get manifest(): ManifestTiptapToolbarExtensionMenuKind | undefined {
		return this.#manifest;
	}
	#manifest?: ManifestTiptapToolbarExtensionMenuKind | undefined;

	override connectedCallback() {
		super.connectedCallback();

		if (this.editor) {
			this.editor.on('selectionUpdate', this.#onEditorUpdate);
			this.editor.on('update', this.#onEditorUpdate);
		}
	}

	override disconnectedCallback() {
		super.disconnectedCallback();

		if (this.editor) {
			this.editor.off('selectionUpdate', this.#onEditorUpdate);
			this.editor.off('update', this.#onEditorUpdate);
		}
	}

	async #setMenu() {
		const items = this.#manifest?.items ?? this.#manifest?.meta.items;
		if (!items) return;
		this.#menu = await this.#getMenuItems(items);
	}

	async #getMenuItems(items: Array<MetaTiptapToolbarMenuItem>): Promise<Array<UmbCascadingMenuItem>> {
		const menu = [];

		for (const item of items) {
			const menuItem = await this.#getMenuItem(item);
			menu.push(menuItem);
		}

		return menu;
	}

	async #getMenuItem(item: MetaTiptapToolbarMenuItem): Promise<UmbCascadingMenuItem> {
		let element;

		// TODO: Commented out as needs review of how async/await is being handled here. [LK]
		// if (item.element) {
		// 	const elementConstructor = await loadManifestElement(item.element);
		// 	if (elementConstructor) {
		// 		element = new elementConstructor();
		// 	}
		// }

		if (!element && item.elementName) {
			element = document.createElement(item.elementName);
		}

		if (element) {
			// TODO: Enforce a type for the element, that has an `editor` property. [LK]
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			element.editor = this.editor;
		}

		let items;
		if (item.items) {
			items = await this.#getMenuItems(item.items);
		}

		return {
			icon: item.appearance?.icon ?? item.icon,
			items,
			label: item.label,
			menu: item.menu,
			style: item.appearance?.style ?? item.style,
			separatorAfter: item.separatorAfter,
			element,
			isActive: () => this.api?.isActive(this.editor, item),
			execute: () => this.api?.execute(this.editor, item),
		};
	}

	#isMenuActive(items?: UmbCascadingMenuItem[]): boolean {
		return !!items?.some((item) => item.isActive?.() || this.#isMenuActive(item.items));
	}

	readonly #onEditorUpdate = () => {
		if (this.api && this.editor && this.manifest) {
			this.isActive = this.api.isActive(this.editor) || this.#isMenuActive(this.#menu) || false;
		}
	};

	override render() {
		const label = this.localize.string(this.manifest?.meta.label);
		const disabled = this.api?.isDisabled(this.editor);
		return html`
			${when(
				this.manifest?.meta.look === 'icon',
				() => html`
					<uui-button
						compact
						label=${label}
						look=${this.isActive ? 'outline' : 'default'}
						title=${label}
						popovertarget="popover-menu"
						?disabled=${disabled}>
						${when(
							this.manifest?.meta.icon,
							(icon) => html`<umb-icon name=${icon}></umb-icon>`,
							() => html`<span>${label}</span>`,
						)}
						<uui-symbol-expand slot="extra" open></uui-symbol-expand>
					</uui-button>
				`,
				() => html`
					<uui-button
						compact
						label=${label}
						look=${this.isActive ? 'outline' : 'default'}
						popovertarget="popover-menu"
						?disabled=${disabled}>
						<span>${label}</span>
						<uui-symbol-expand slot="extra" open></uui-symbol-expand>
					</uui-button>
				`,
			)}
			${this.renderMenu()}
		`;
	}

	protected renderMenu() {
		return html`
			<umb-cascading-menu-popover id="popover-menu" placement="bottom-start" .items=${this.#menu}>
				${when(
					this.#manifest?.menu,
					(menuAlias) => html`
						<umb-extension-slot
							type="menu"
							default-element="umb-tiptap-menu"
							single
							.filter=${(menu: ManifestMenu) => menu.alias === menuAlias}></umb-extension-slot>
					`,
				)}
			</umb-cascading-menu-popover>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				--uui-button-font-weight: normal;
				--uui-menu-item-flat-structure: 1;

				margin-left: var(--uui-size-space-1);
				margin-bottom: var(--uui-size-space-1);
			}

			uui-button > uui-symbol-expand {
				margin-left: var(--uui-size-space-2);
			}
		`,
	];
}

export { UmbTiptapToolbarMenuElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tiptap-toolbar-menu': UmbTiptapToolbarMenuElement;
	}
}
