import { html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import {
	ManifestKind,
	ManifestMenuItemTreeKind,
	UmbMenuItemExtensionElement,
} from '@umbraco-cms/backoffice/extensions-registry';

// TODO: Move to separate file:
const manifest: ManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Tree',
	matchKind: 'tree',
	matchType: 'menuItem',
	manifest: {
		type: 'menuItem',
		elementName: 'umb-menu-item-tree',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-menu-item-tree')
export class UmbMenuItemTreeElement extends UmbLitElement implements UmbMenuItemExtensionElement {
	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	render() {
		return this.manifest ? html` <umb-tree alias=${this.manifest?.meta.treeAlias}></umb-tree> ` : nothing;
	}
}

export default UmbMenuItemTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-tree': UmbMenuItemTreeElement;
	}
}
