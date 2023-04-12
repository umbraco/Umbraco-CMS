import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbEntityTreeItemContext } from './entity-tree-item.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { ManifestKind } from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

// TODO: Move to separate file:
const manifest: ManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityTreeItem',
	matchKind: 'entity',
	matchType: 'treeItem',
	manifest: {
		type: 'treeItem',
		elementName: 'umb-entity-tree-item',
	},
};
umbExtensionsRegistry.register(manifest);

@customElement('umb-entity-tree-item')
export class UmbEntityTreeItemElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	private _item?: EntityTreeItemResponseModel;
	@property({ type: Object, attribute: false })
	public get item() {
		return this._item;
	}
	public set item(value: EntityTreeItemResponseModel | undefined) {
		this._item = value;
		this.#context.setTreeItem(value);
	}

	#context = new UmbEntityTreeItemContext(this);

	render() {
		if (!this.item) return nothing;
		return html`<umb-tree-item-base></umb-tree-item-base>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-tree-item': UmbEntityTreeItemElement;
	}
}
