import type { UmbTreeItemModelBase } from '../types.js';
import { css, html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-tree-item-default')
export class UmbTreeItemDefaultElement extends UmbLitElement {
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
		'umb-tree-item-default': UmbTreeItemDefaultElement;
	}
}
