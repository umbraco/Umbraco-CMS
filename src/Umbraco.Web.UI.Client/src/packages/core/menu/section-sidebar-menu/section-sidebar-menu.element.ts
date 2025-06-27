import type { ManifestMenu } from '../menu.extension.js';
import type { ManifestSectionSidebarAppBaseMenu, ManifestSectionSidebarAppMenuKind } from './types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_SECTION_SIDEBAR_MENU_CONTEXT } from './context/section-sidebar-menu.context.token.js';

// TODO: Move to separate file:
const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.SectionSidebarAppMenu',
	matchKind: 'menu',
	matchType: 'sectionSidebarApp',
	manifest: {
		type: 'sectionSidebarApp',
		elementName: 'umb-section-sidebar-menu',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-section-sidebar-menu')
export class UmbSectionSidebarMenuElement<
	ManifestType extends ManifestSectionSidebarAppBaseMenu = ManifestSectionSidebarAppMenuKind,
> extends UmbLitElement {
	@property({ type: Object, attribute: false })
	manifest?: ManifestType;

	renderHeader() {
		/*
		Notice we are not using the UUI-H3 here, we would need to wrap it into uui-text for this to take action,
		but it does look odd so lets return to this later. I have made a few corrections especially for this component. [NL]
		*/
		return html`<h3>${this.localize.string(this.manifest?.meta?.label ?? '')}</h3>`;
	}

	#context?: typeof UMB_SECTION_SIDEBAR_MENU_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_CONTEXT, (context) => {
			this.#context = context;
			this.#observeExpansion();
		});
	}

	#observeExpansion() {
		this.observe(this.#context?.expansion.expansion, (items) => {
			console.log('Expanded items:', items);
		});
	}

	override render() {
		return html`
			${this.renderHeader()}
			<umb-extension-slot
				type="menu"
				.filter="${(menu: ManifestMenu) => menu.alias === this.manifest?.meta?.menu}"
				default-element="umb-menu"></umb-extension-slot>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			h3 {
				margin: var(--uui-size-5) 0;
				padding: var(--uui-size-4) var(--uui-size-8);
				font-size: 14px;
			}
		`,
	];
}

export default UmbSectionSidebarMenuElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-menu': UmbSectionSidebarMenuElement;
	}
}
