import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';

const elementName = 'umb-user-create-options-modal';
@customElement(elementName)
export class UmbUserCreateOptionsModalElement extends UmbModalBaseElement {
	@state()
	entity: UmbEntityModel = {
		entityType: '',
		unique: '',
	};

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.entity = {
				entityType: context.getEntityType(),
				unique: context.getUnique(),
			};
		});
	}

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('user_createUser')}">
				<uui-box>
					<umb-entity-create-option-action-list
						.entityType=${this.entity.entityType}
						.unique=${this.entity.unique}></umb-entity-create-option-action-list>
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
			</umb-body-layout>
		`;
	}
}

export { UmbUserCreateOptionsModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbUserCreateOptionsModalElement;
	}
}
