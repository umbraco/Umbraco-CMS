import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_TREE_ITEM_CONTEXT } from '@umbraco-cms/backoffice/tree';

@customElement('umb-recently-created-sign')
export class UmbRecentlyCreatedSignElement extends UmbLitElement {
	@state() private _createDate?: string;

	override connectedCallback(): void {
		super.connectedCallback();
		this.consumeContext(UMB_TREE_ITEM_CONTEXT, (ctx) => {
			const item = ctx?.getTreeItem?.() ?? (ctx as any).item;
			this._createDate = item?.createDate;
			this.requestUpdate();
		});
	}

	protected override render() {
		if (!this._createDate) return null;
		const created = new Date(this._createDate);
		const weekAgo = new Date();
		weekAgo.setDate(weekAgo.getDate() - 7);
		return created > weekAgo ? html`<div>New</div>` : null;
	}
}

export default UmbRecentlyCreatedSignElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-recently-created-sign': UmbRecentlyCreatedSignElement;
	}
}
