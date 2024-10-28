import type {
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue,
} from './entity-create-option-action-list-modal.token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

const elementName = 'umb-entity-create-option-action-list-modal';
@customElement(elementName)
export class UmbEntityCreateOptionActionListModalElement extends UmbModalBaseElement<
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue
> {
	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('user_createUser')}">
				<uui-box>
					<umb-entity-create-option-action-list
						.entityType=${this.data?.entityType}
						.unique=${this.data?.unique}></umb-entity-create-option-action-list>
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
			</umb-body-layout>
		`;
	}
}

export { UmbEntityCreateOptionActionListModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityCreateOptionActionListModalElement;
	}
}
