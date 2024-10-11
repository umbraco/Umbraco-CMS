import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-entity-actions-table-column-layout';
@customElement(elementName)
export class UmbEntityActionsTableColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value?: UmbEntityModel;

	@state()
	_isOpen = false;

	#onActionExecuted() {
		this._isOpen = false;
	}

	override render() {
		if (!this.value) return nothing;

		return html`
			<umb-dropdown .open=${this._isOpen} compact hide-expand>
				<uui-symbol-more slot="label"></uui-symbol-more>
				<umb-entity-action-list
					@action-executed=${this.#onActionExecuted}
					entity-type=${this.value.entityType}
					.unique=${this.value.unique}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityActionsTableColumnLayoutElement;
	}
}
