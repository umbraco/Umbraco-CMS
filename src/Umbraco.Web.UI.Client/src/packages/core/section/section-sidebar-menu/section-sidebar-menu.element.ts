import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import {
	ManifestMenu,
	ManifestSectionSidebarAppBaseMenu,
	ManifestSectionSidebarAppMenuKind,
	UmbBackofficeManifestKind,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// TODO: Move to separate file:
const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Menu',
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
	ManifestType extends ManifestSectionSidebarAppBaseMenu = ManifestSectionSidebarAppMenuKind
> extends UmbLitElement {
	@property({type: Object, attribute: false})
	manifest?: ManifestType;

	renderHeader() {
		return html`<h3>${this.manifest?.meta?.label}</h3>`;
	}

	render() {
		// TODO: link to dashboards when clicking on the menu item header
		return html`${this.renderHeader()}
			<umb-extension-slot
				type="menu"
				.filter=${(menu: ManifestMenu) => menu.alias === this.manifest?.meta?.menu}
				default-element="umb-menu"></umb-extension-slot>`;
	}

	static styles = [
		UUITextStyles,
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
