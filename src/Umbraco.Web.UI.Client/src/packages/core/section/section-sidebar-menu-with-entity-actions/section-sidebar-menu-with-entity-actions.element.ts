import { UmbSectionSidebarMenuElement } from '../section-sidebar-menu/section-sidebar-menu.element.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import {
	ManifestSectionSidebarAppMenuWithEntityActionsKind,
	UmbBackofficeManifestKind,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';

import '../../menu/menu.element.js';

const manifestWithEntityActions: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Menu',
	matchKind: 'menuWithEntityActions',
	matchType: 'sectionSidebarApp',
	manifest: {
		type: 'sectionSidebarApp',
		elementName: 'umb-section-sidebar-menu-with-entity-actions',
	},
};
umbExtensionsRegistry.register(manifestWithEntityActions);

@customElement('umb-section-sidebar-menu-with-entity-actions')
export class UmbSectionSidebarMenuWithEntityActionsElement extends UmbSectionSidebarMenuElement<ManifestSectionSidebarAppMenuWithEntityActionsKind> {
	renderHeader() {
		return html`<div id="header">
			<h3>${this.manifest?.meta?.label}</h3>
			<umb-entity-actions-bundle
				slot="actions"
				.unique=${null}
				.entityType=${this.manifest?.meta.entityType}
				.label=${this.manifest?.meta.label}>
			</umb-entity-actions-bundle>
		</div> `;
	}

	static styles = [
		...UmbSectionSidebarMenuElement.styles,
		css`
			#header {
				display: flex;
				flex-direction: row;
				align-items: center;
			}
			#header > :first-child {
				flex-grow: 1;
			}
		`,
	];
}

export default UmbSectionSidebarMenuWithEntityActionsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-sidebar-menu-with-entity-actions': UmbSectionSidebarMenuElement;
	}
}
