import { UmbSectionSidebarMenuElement } from '../section-sidebar-menu/section-sidebar-menu.element.js';
import type { ManifestSectionSidebarAppMenuWithEntityActionsKind } from '../section-sidebar-menu/types.js';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const manifestWithEntityActions: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.SectionSidebarAppMenuWithEntityActions',
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
	override renderHeader() {
		return html`
			<div id="header">
				<h3>${this.localize.string(this.manifest?.meta?.label ?? '')}</h3>
				<umb-entity-actions-bundle
					slot="actions"
					.unique=${null}
					.entityType=${this.manifest?.meta.entityType}
					.label=${this.localize.term('actions_viewActionsFor', [this.manifest?.meta.label])}>
				</umb-entity-actions-bundle>
			</div>
		`;
	}

	static override styles = [
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
