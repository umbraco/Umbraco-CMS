import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbSectionSidebarMenuElement } from '../section-sidebar-menu/section-sidebar-menu.element';
import {
	ManifestSectionSidebarAppMenuWithEntityActionsKind,
	UmbBackofficeManifestKind,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';

import '../../menu/menu.element';

const manifestWithEntityActions: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Menu',
	matchKind: 'menuWithEntityActions',
	matchType: 'sectionSidebarApp',
	manifest: {
		type: 'sectionSidebarApp',
		elementName: 'umb-section-sidebar-menu',
	},
};
umbExtensionsRegistry.register(manifestWithEntityActions);

@customElement('umb-section-sidebar-menu-with-entity-actions')
export class UmbSectionSidebarMenuWithEntityActionsElement extends UmbSectionSidebarMenuElement<ManifestSectionSidebarAppMenuWithEntityActionsKind> {
	renderHeader() {
		return html`<h3>${this.manifest?.meta?.label}</h3>
			<umb-entity-actions-bundle
				slot="actions"
				entity-type=${this.manifest?.meta.entityType}
				.label=${this.manifest?.meta.label}>
			</umb-entity-actions-bundle> `;
	}
}

export default UmbSectionSidebarMenuWithEntityActionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-menu-with-entity-actions': UmbSectionSidebarMenuElement;
	}
}
