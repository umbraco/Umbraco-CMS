import { UmbTreeItemElementBase } from '../tree-item-base/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-default-tree-item')
export class UmbDefaultTreeItemElement extends UmbTreeItemElementBase {}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree-item': UmbDefaultTreeItemElement;
	}
}
