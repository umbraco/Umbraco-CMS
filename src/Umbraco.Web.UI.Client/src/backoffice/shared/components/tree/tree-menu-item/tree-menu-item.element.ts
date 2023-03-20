import { html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestKind, ManifestMenuItem } from '@umbraco-cms/extensions-registry';

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

export interface ManifestMenuItemTreeKind extends Omit<Omit<ManifestMenuItem, 'kind'>, 'meta'> {
	type: 'menuItem';
	kind: 'tree';
	meta: MetaMenuItemTreeKind;
}

export interface MetaMenuItemTreeKind {
	treeAlias: string;
	label: string;
	icon: string;
	entityType?: string;
}

@customElement('umb-menu-item-tree')
export class UmbMenuItemTreeElement extends UmbLitElement {
	@state()
	private _renderTree = false;

	private _onShowChildren() {
		this._renderTree = true;
	}

	private _onHideChildren() {
		this._renderTree = false;
	}

	@property({ type: Object })
	manifest?: ManifestMenuItemTreeKind;

	// TODO: check if root has children before settings the has-children attribute
	// TODO: how do we want to cache the tree? (do we want to rerender every time the user opens the tree)?
	render() {
		return this.manifest
			? html`
					<uui-menu-item
						href=""
						label=${this.manifest?.meta.label}
						@show-children=${this._onShowChildren}
						@hide-children=${this._onHideChildren}
						has-children
						><uui-icon slot="icon" name=${this.manifest?.meta.icon}></uui-icon>
						${this._renderTree ? html`<umb-tree alias=${this.manifest?.meta.treeAlias}></umb-tree>` : nothing}
					</uui-menu-item>
			  `
			: '';
	}
}

export default UmbMenuItemTreeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-menu-item-tree': UmbMenuItemTreeElement;
	}
}
