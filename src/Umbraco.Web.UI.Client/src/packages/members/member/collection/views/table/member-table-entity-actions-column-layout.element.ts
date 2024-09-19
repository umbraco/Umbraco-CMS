import type { UmbMemberEntityType } from '../../../entity.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

const elementName = 'umb-member-table-entity-actions-column-layout';
@customElement(elementName)
export class UmbMemberTableEntityActionsColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value!: {
		unique: string;
		entityType: UmbMemberEntityType;
	};

	@state()
	_isOpen = false;

	#onActionExecuted() {
		this._isOpen = false;
	}

	override render() {
		return html`
			<umb-dropdown .open=${this._isOpen} compact hide-expand>
				<uui-symbol-more slot="label"></uui-symbol-more>
				<umb-entity-action-list
					@action-executed=${this.#onActionExecuted}
					entity-type=${this.value.entityType}
					unique=${ifDefined(this.value.unique)}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbMemberTableEntityActionsColumnLayoutElement;
	}
}
