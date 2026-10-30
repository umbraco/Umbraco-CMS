import { UMB_TREE_ITEM_PICKER_CONTEXT } from '../../tree-item-picker/tree-item-picker.context-token.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
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
 * The last item is where the picker is now. It does not browse, but when `selectable` is set it toggles the selection of
 * that location — the only way to choose it when the host lists just its children.
 * @element umb-tree-item-picker-breadcrumb
 * @fires {UmbSelectedEvent} selected - When the location being browsed is chosen.
 * @fires {UmbDeselectedEvent} deselected - When the location being browsed is chosen again, while selected.
 */
@customElement('umb-tree-item-picker-breadcrumb')
export class UmbTreeItemPickerBreadcrumbElement extends UmbLitElement {
	/**
	 * Whether the location being browsed can be chosen.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean })
	selectable = false;

	@state()
	private _items: Array<UmbTreeItemPickerBreadcrumbItem> = [];

	@state()
	private _selection: Array<string | null> = [];

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
			this.observe(
				context?.selection.selection,
				(selection) => {
					this._selection = selection ?? [];
				},
				'umbTreeItemPickerBreadcrumbSelectionObserver',
			);
		});
	}

	#onItemClick(item: UmbTreeItemPickerBreadcrumbItem, index: number) {
		if (index === this._items.length - 1) return;
		// The root carries no unique, and browsing to it means browsing to wherever the picker starts.
		this.#context?.location.navigateTo(item.unique ? { unique: item.unique, entityType: item.entityType } : undefined);
	}

	#onLocationClick(item: UmbTreeItemPickerBreadcrumbItem) {
		const event = this._selection.includes(item.unique)
			? new UmbDeselectedEvent(item.unique)
			: new UmbSelectedEvent(item.unique);
		this.dispatchEvent(event);
	}

	override render() {
		if (!this._items.length) return nothing;

		return html`
			<uui-breadcrumbs>
				${this._items.map((item, index) => {
					const isLast = index === this._items.length - 1;
					return html`
						<uui-breadcrumb-item ?last-item=${isLast} @click=${() => this.#onItemClick(item, index)}>
							${isLast && this.selectable ? this.#renderLocation(item) : this.localize.string(item.name)}
						</uui-breadcrumb-item>
					`;
				})}
			</uui-breadcrumbs>
		`;
	}

	#renderLocation(item: UmbTreeItemPickerBreadcrumbItem) {
		return html`<button
			type="button"
			aria-pressed=${this._selection.includes(item.unique) ? 'true' : 'false'}
			@click=${() => this.#onLocationClick(item)}>
			${this.localize.string(item.name)}
		</button>`;
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

		button {
			font: inherit;
			color: inherit;
			background: none;
			border: 0;
			border-radius: var(--uui-border-radius);
			padding: 0 var(--uui-size-space-1);
			max-width: 100%;
			overflow: hidden;
			text-overflow: ellipsis;
			cursor: pointer;
		}

		button:hover {
			text-decoration: underline;
		}

		button[aria-pressed='true'] {
			background-color: var(--uui-color-selected);
			color: var(--uui-color-selected-contrast);
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-picker-breadcrumb': UmbTreeItemPickerBreadcrumbElement;
	}
}
