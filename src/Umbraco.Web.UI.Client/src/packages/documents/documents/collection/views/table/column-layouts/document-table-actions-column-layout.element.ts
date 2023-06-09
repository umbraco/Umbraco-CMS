import {
	css,
	html,
	LitElement,
	nothing,
	ifDefined,
	customElement,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import type { UmbTableColumn, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbExecutedEvent } from '@umbraco-cms/backoffice/events';

// TODO: this could be done more generic, but for now we just need it for the document table
@customElement('umb-document-table-actions-column-layout')
export class UmbDocumentTableActionColumnLayoutElement extends LitElement {
	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: any;

	@state()
	private _actionMenuIsOpen = false;

	#close() {
		this._actionMenuIsOpen = false;
	}

	#open(event: Event) {
		event.stopPropagation();
		this._actionMenuIsOpen = true;
	}

	#onActionExecuted(event: UmbExecutedEvent) {
		event.stopPropagation();
		this.#close();
	}

	render() {
		return html`
			<uui-popover id="action-menu-popover" .open=${this._actionMenuIsOpen} @close=${this.#close}>
				<uui-button slot="trigger" compact @click=${this.#open}><uui-symbol-more></uui-symbol-more></uui-button>
				${this._actionMenuIsOpen ? this.#renderDropdown() : nothing}
			</uui-popover>
		`;
	}

	#renderDropdown() {
		return html`
			<div id="action-menu-dropdown" slot="popover">
				<uui-scroll-container>
					<umb-entity-action-list
						@executed=${this.#onActionExecuted}
						entity-type=${ifDefined(this.value.entityType)}
						unique=${ifDefined(this.item.id)}></umb-entity-action-list>
				</uui-scroll-container>
			</div>
		`;
	}

	static styles = [
		css`
			#action-menu-popover {
				display: block;
				text-align: right;
			}
			#action-menu-dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
				width: 500px;
			}
		`,
	];
}

export default UmbDocumentTableActionColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-actions-column-layout': UmbDocumentTableActionColumnLayoutElement;
	}
}
