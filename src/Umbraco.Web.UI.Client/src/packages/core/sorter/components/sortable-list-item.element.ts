import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-sortable-list-item
 * @slot - The content of the item.
 * @slot actions - Actions to render for the item, e.g. a remove button.
 */
@customElement('umb-sortable-list-item')
export class UmbSortableListItemElement extends UmbLitElement {
	/**
	 * The item's unique identifier, used by the parent `umb-sortable-list` to track it while sorting.
	 * @type {string}
	 */
	@property({ attribute: 'data-sort-entry-id', reflect: true })
	public unique?: string;

	/**
	 * Disables the item, hiding the drag handle and actions.
	 * @type {boolean}
	 * @attr
	 */
	@property({ type: Boolean, reflect: true })
	disabled = false;

	/**
	 * Hides the drag handle.
	 * @type {boolean}
	 * @attr hide-handle
	 */
	@property({ type: Boolean, attribute: 'hide-handle' })
	hideHandle = false;

	/**
	 * Hides the actions slot.
	 * @type {boolean}
	 * @attr hide-actions
	 */
	@property({ type: Boolean, attribute: 'hide-actions' })
	hideActions = false;

	override render() {
		return html`
			${this.#renderHandle()}
			<slot></slot>
			${this.#renderActions()}
		`;
	}

	#renderHandle() {
		if (this.hideHandle || this.disabled) return nothing;
		return html`<uui-icon name="icon-grip" class="handle"></uui-icon>`;
	}

	#renderActions() {
		if (this.hideActions || this.disabled) return nothing;
		return html`<div class="actions"><slot name="actions"></slot></div>`;
	}

	static override readonly styles = [
		css`
			:host {
				display: flex;
				align-items: center;
				gap: var(--uui-size-6);
				padding: var(--uui-size-3) 0;
				background-color: var(--uui-color-surface);
			}

			:host([drag-placeholder]) {
				opacity: 0.5;
			}

			.handle {
				flex: 0 0 var(--uui-size-6);
				cursor: grab;

				&:active {
					cursor: grabbing;
				}
			}

			::slotted(*) {
				flex: 1;
			}

			.actions {
				flex: 0 0 auto;
				display: flex;
				justify-content: flex-end;
			}
		`,
	];
}

export default UmbSortableListItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-sortable-list-item': UmbSortableListItemElement;
	}
}
