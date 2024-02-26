import type { UmbTreeItemModelBase } from '../types.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-item')
export class UmbTreeItemElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	item?: UmbTreeItemModelBase;

	render() {
		if (!this.item) return nothing;
		return html`<umb-extension-slot
			type="treeItem"
			.filter=${(manifests: ManifestTreeItem) => manifests.meta.entityTypes.includes(this.item!.entityType)}
			.props=${{
				item: this.item,
			}}></umb-extension-slot>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item': UmbTreeItemElement;
	}
}
