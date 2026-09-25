import { UMB_TREE_ITEM_PICKER_CONTEXT } from '../../tree-item-picker/tree-item-picker.context-token.js';
import { css, customElement, html, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export interface UmbTreeItemPickerBreadcrumbItem {
	unique: string | null;
	entityType: string;
	name: string;
}

/**
 * The trail of the location a picker is browsing. It takes the trail from the picker context and browses it, so a host
 * only decides where to put it.
 *
 * The last item is where the picker is now and is not clickable.
 * @element umb-tree-item-picker-breadcrumb
 */
@customElement('umb-tree-item-picker-breadcrumb')
export class UmbTreeItemPickerBreadcrumbElement extends UmbLitElement {
	@state()
	private _items: Array<UmbTreeItemPickerBreadcrumbItem> = [];

	#context?: typeof UMB_TREE_ITEM_PICKER_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_TREE_ITEM_PICKER_CONTEXT, (context) => {
			this.#context = context;
			this.observe(
				context?.location.breadcrumb,
				(breadcrumb) => {
					this._items = breadcrumb ?? [];
				},
				'umbTreeItemPickerBreadcrumbObserver',
			);
		});
	}

	#onItemClick(item: UmbTreeItemPickerBreadcrumbItem, index: number) {
		if (index === this._items.length - 1) return;
		// The root carries no unique, and browsing to it means browsing to wherever the picker starts.
		this.#context?.location.navigateTo(item.unique ? { unique: item.unique, entityType: item.entityType } : undefined);
	}

	override render() {
		if (!this._items.length) return nothing;

		return html`
			<uui-breadcrumbs>
				${this._items.map(
					(item, index) => html`
						<uui-breadcrumb-item
							?last-item=${index === this._items.length - 1}
							@click=${() => this.#onItemClick(item, index)}>
							${this.localize.string(item.name)}
						</uui-breadcrumb-item>
					`,
				)}
			</uui-breadcrumbs>
		`;
	}

	static override styles = css`
		:host {
			display: block;
		}

		uui-breadcrumbs {
			overflow: hidden;
			min-width: 0;
		}

		uui-breadcrumb-item:not([last-item]) {
			cursor: pointer;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-picker-breadcrumb': UmbTreeItemPickerBreadcrumbElement;
	}
}
