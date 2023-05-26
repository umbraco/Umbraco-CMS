import { UmbFileSystemTreeItemContext } from './file-system-tree-item.context.js';
import { css, html, nothing , customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbTreeItemElement } from '@umbraco-cms/backoffice/tree';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbBackofficeManifestKind, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: Move to separate file:
const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.FileSystemTreeItem',
	matchKind: 'fileSystem',
	matchType: 'treeItem',
	manifest: {
		type: 'treeItem',
		elementName: 'umb-file-system-tree-item',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-file-system-tree-item')
export class UmbFileSystemTreeItemElement extends UmbLitElement implements UmbTreeItemElement {
	private _item?: FileSystemTreeItemPresentationModel;
	@property({ type: Object, attribute: false })
	public get item() {
		return this._item;
	}
	public set item(value: FileSystemTreeItemPresentationModel | undefined) {
		this._item = value;
		this.#context.setTreeItem(value);
	}

	#context = new UmbFileSystemTreeItemContext(this);

	render() {
		if (!this.item) return nothing;
		return html`<umb-tree-item-base></umb-tree-item-base>`;
	}

	static styles = [UUITextStyles, css``];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-file-system-tree-item': UmbFileSystemTreeItemElement;
	}
}
