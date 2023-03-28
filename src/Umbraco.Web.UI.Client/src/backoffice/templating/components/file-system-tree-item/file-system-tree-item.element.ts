import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbFileSystemTreeItemContext } from './file-system-tree-item.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestKind } from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { FileSystemTreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: Move to separate file:
const manifest: ManifestKind = {
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
export class UmbFileSystemTreeItemElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-file-system-tree-item': UmbFileSystemTreeItemElement;
	}
}
