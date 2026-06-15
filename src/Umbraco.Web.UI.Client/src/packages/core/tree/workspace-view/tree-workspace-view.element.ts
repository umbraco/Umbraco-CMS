import { UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { ManifestWorkspaceViewTreekind } from './types.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTreeStartNode } from '../types.js';

@customElement('umb-tree-workspace-view')
export class UmbTreeWorkspaceViewElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	public manifest?: ManifestWorkspaceViewTreekind;

	@state()
	private _parent?: UmbTreeStartNode;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			const entityType = context?.getEntityType();
			const unique = context?.getUnique();
			this._parent = entityType && unique ? { entityType, unique } : undefined;
		});
	}

	override render() {
		if (!this.manifest) return html` <div>No Manifest</div>`;
		if (!this.manifest.meta.treeAlias) return html` <div>No Collection Alias in Manifest</div>`;
		return html`<umb-tree
			data-mark="tree:${this.manifest.meta.treeAlias}"
			alias=${this.manifest.meta.treeAlias}
			.props=${{
				hideToolbar: false,
				hideTreeRoot: true,
				startNode: this._parent,
				selectionConfiguration: {
					selectable: false,
				},
			}}></umb-tree>`;
	}
}

export { UmbTreeWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-workspace-view': UmbTreeWorkspaceViewElement;
	}
}
