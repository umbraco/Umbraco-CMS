import { UMB_TREE_CONTEXT } from '../../constants.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../../types.js';
import { css, customElement, html, ifDefined, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-card-view')
export class UmbTreeCardViewElement extends UmbLitElement {
	@state()
	private _items: Array<UmbTreeItemModel | UmbTreeRootModel> | undefined;

	#treeContext?: typeof UMB_TREE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.#treeContext = context;
			this.#observeData();
		});
	}

	#observeData() {
		this.observe(this.#treeContext?.items, (items) => (this._items = items));
	}

	#expand() {
		console.log('expand');
	}

	override render() {
		return html`${this.#renderItems()}`;
	}

	#renderItems() {
		if (!this._items) return nothing;
		console.log(this._items);

		return html`
			<div id="tree-card-grid">
				${repeat(
					this._items,
					(item, index) => item.name + '___' + index,
					(item) =>
						html` <uui-card
							><umb-icon name=${ifDefined(item.icon)}></umb-icon>
							<h3>${item.name}</h3>
							${item.hasChildren ? html`<uui-button @click=${this.#expand}>Expand</uui-button>` : nothing}
						</uui-card>`,
				)}
			</div>
		`;
	}

	static override styles = css`
		#tree-card-grid {
			display: grid;
			gap: var(--uui-size-space-5);
			grid-template-columns: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
			grid-auto-rows: var(--umb-card-medium-min-width);
			padding-bottom: 5px;
		}
	`;
}

export { UmbTreeCardViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-card-view': UmbTreeCardViewElement;
	}
}
