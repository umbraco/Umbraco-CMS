import { UmbTreeItemElementBase } from '../tree-item-base/index.js';
import type { UmbTreeItemModel } from '../../types.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-default-tree-item')
export class UmbDefaultTreeItemElement extends UmbTreeItemElementBase<UmbTreeItemModel> {}

export default UmbDefaultTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree-item': UmbDefaultTreeItemElement;
	}
}
