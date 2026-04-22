import type { UmbTreeItemModel } from '../../types.js';
import type { UmbTreeContext } from '../../tree.context.interface.js';
import { UMB_TREE_CONTEXT } from '../../tree.context.token.js';
import { getItemFallbackIcon } from '@umbraco-cms/backoffice/entity-item';
import { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-default-tree-item-card')
export class UmbDefaultTreeItemCardElement extends UmbLitElement {
	#treeContext?: UmbTreeContext;

	@property({ type: Object, attribute: false })
	item?: UmbTreeItemModel;

	@property({ type: Boolean })
	selectable = false;

	@property({ type: Boolean })
	selectOnly = false;

	@property({ type: Boolean })
	selected = false;

	constructor() {
		super();
		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this.#treeContext = context;
		});
	}

	#onSelected(e: CustomEvent) {
		if (!this.item) return;
		e.stopPropagation();
		this.dispatchEvent(new UmbSelectedEvent(this.item.unique));
	}

	#onDeselected(e: CustomEvent) {
		if (!this.item) return;
		e.stopPropagation();
		this.dispatchEvent(new UmbDeselectedEvent(this.item.unique));
	}

	#onDblClick(e: MouseEvent) {
		if (!this.item?.hasChildren) return;
		e.stopPropagation();
		this.#treeContext?.open(this.item);
	}

	#onKeyDown(e: KeyboardEvent) {
		if (e.key === 'ArrowRight' && this.item?.hasChildren) {
			e.stopPropagation();
			this.#treeContext?.open(this.item);
		}
	}

	override render() {
		if (!this.item) return nothing;
		return html`
			<umb-figure-card
				name=${this.item.name}
				?selectable=${this.selectable}
				?select-only=${this.selectOnly}
				?selected=${this.selected}
				background-color="var(--uui-color-surface)"
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}
				@dblclick=${this.#onDblClick}
				@keydown=${this.#onKeyDown}>
				${this.#renderIcon(this.item)}
			</umb-figure-card>
		`;
	}

	#renderIcon(item: UmbTreeItemModel) {
		const icon = item.isFolder ? 'icon-folder' : item.icon || getItemFallbackIcon();
		return html`<umb-icon name=${icon}></umb-icon>`;
	}
}

export default UmbDefaultTreeItemCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-tree-item-card': UmbDefaultTreeItemCardElement;
	}
}
