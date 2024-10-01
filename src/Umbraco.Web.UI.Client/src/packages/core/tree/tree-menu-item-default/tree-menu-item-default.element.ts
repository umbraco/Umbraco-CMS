import type { ManifestMenuItemTreeKind } from './types.js';
import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbMenuItemElement } from '@umbraco-cms/backoffice/menu';

// TODO: Move to separate file:
const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Tree',
	matchKind: 'tree',
	matchType: 'menuItem',
	manifest: {
		type: 'menuItem',
		elementName: 'umb-menu-item-tree-default',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-menu-item-tree-default')
export class UmbMenuItemTreeDefaultElement extends UmbLitElement implements UmbMenuItemElement {
	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	override render() {
		return this.manifest
			? html`
					<umb-tree
						alias=${this.manifest?.meta.treeAlias}
						.props=${{
							hideTreeRoot: this.manifest?.meta.hideTreeRoot === true,
							selectionConfiguration: {
								selectable: false,
								multiple: false,
							},
						}}></umb-tree>
				`
			: nothing;
	}
}

export default UmbMenuItemTreeDefaultElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-tree-default': UmbMenuItemTreeDefaultElement;
	}
}
