import type { UmbElementTreeItemModel } from '../types.js';
import type { UmbElementTreeItemContext } from './element-tree-item.context.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-element-tree-item')
export class UmbElementTreeItemElement extends UmbTreeItemElementBase<
	UmbElementTreeItemModel,
	UmbElementTreeItemContext
> {
	override renderLabel() {
		return html`<span id="label" slot="label">${this.item?.name || ''}</span> `;
	}

	static override styles = UmbTreeItemElementBase.styles;
}

export default UmbElementTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-tree-item': UmbElementTreeItemElement;
	}
}
