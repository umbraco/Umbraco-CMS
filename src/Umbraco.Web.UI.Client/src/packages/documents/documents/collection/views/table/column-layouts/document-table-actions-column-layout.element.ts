import { css, html, LitElement, ifDefined, customElement, property, query } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDropdownElement, UmbTableColumn, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbActionExecutedEvent } from '@umbraco-cms/backoffice/event';

// TODO: this could be done more generic, but for now we just need it for the document table
@customElement('umb-document-table-actions-column-layout')
export class UmbDocumentTableActionColumnLayoutElement extends LitElement {
	@query('#document-layout-dropdown')
	dropdownElement?: UmbDropdownElement;

	@property({ type: Object, attribute: false })
	column!: UmbTableColumn;

	@property({ type: Object, attribute: false })
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: any;

	#onActionExecuted(event: UmbActionExecutedEvent) {
		event.stopPropagation();

		if (this.dropdownElement) {
			this.dropdownElement.open = false;
		}
	}

	render() {
		return html`
			<umb-dropdown id="document-layout-dropdown">
				<uui-symbol-more slot="label"></uui-symbol-more>
				<div slot="dropdown">
					<uui-scroll-container>
						<umb-entity-action-list
							@action-executed=${this.#onActionExecuted}
							entity-type=${ifDefined(this.value.entityType)}
							unique=${ifDefined(this.item.id)}></umb-entity-action-list>
					</uui-scroll-container>
				</div>
			</umb-dropdown>
		`;
	}

	static styles = [css``];
}

export default UmbDocumentTableActionColumnLayoutElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-actions-column-layout': UmbDocumentTableActionColumnLayoutElement;
	}
}
