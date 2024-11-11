import type { ManifestMenu } from '../menu.extension.js';
import type { ManifestSectionSidebarAppBaseMenu, ManifestSectionSidebarAppMenuKind } from './types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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
		return html`<h3>${this.localize.string(this.manifest?.meta?.label ?? '')}</h3>`;
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
				padding: var(--uui-size-4) var(--uui-size-8);
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
