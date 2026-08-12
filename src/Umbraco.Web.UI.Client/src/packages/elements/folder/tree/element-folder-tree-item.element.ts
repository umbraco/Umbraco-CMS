import type { UmbElementFolderTreeItemModel } from '../types.js';
import type { UmbElementFolderTreeItemContext } from './element-folder-tree-item.context.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-element-folder-tree-item')
export class UmbElementFolderTreeItemElement extends UmbTreeItemElementBase<
	UmbElementFolderTreeItemModel,
	UmbElementFolderTreeItemContext
> {
}

export { UmbElementFolderTreeItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-folder-tree-item': UmbElementFolderTreeItemElement;
	}
}
