import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { TreeItemPresentationModel } from 'libs/backend-api/src';
import { ManifestTreeItem } from 'libs/extensions-registry/tree-item.models';

@customElement('umb-tree-item')
export class UmbTreeItemElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object, attribute: false })
	item?: TreeItemPresentationModel;

	render() {
		return html`<umb-extension-slot
			type="treeItem"
			.filter=${(manifests: ManifestTreeItem) => manifests.conditions.entityType === this.item?.type}
			.props=${{
				item: this.item,
			}}></umb-extension-slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItemElement;
	}
}
