import { UMB_CREATE_USER_MODAL } from '../../../modals/create/create-user-modal.token.js';
import { UmbUserKind, type UmbUserKindType } from '../../../utils/index.js';
import { html, customElement, map } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';

interface UmbUserCreateOptionModel {
	label: string;
	description?: string;
	icon: string;
	kind: UmbUserKindType;
}

const elementName = 'umb-user-create-options-modal';
@customElement(elementName)
export class UmbUserCreateOptionsModalElement extends UmbModalBaseElement {
	#options: Array<UmbUserCreateOptionModel> = [
		{
			label: this.localize.term('user_userKindDefault'),
			icon: 'icon-user',
			kind: UmbUserKind.DEFAULT,
		},
		{
			label: this.localize.term('user_userKindApi'),
			icon: 'icon-unplug',
			kind: UmbUserKind.API,
		},
	];

	async #onClick(event: Event, kind: UmbUserKindType) {
		event.stopPropagation();
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);

		const unique = entityContext.getUnique();
		const entityType = entityContext.getEntityType();

		if (unique === undefined) throw new Error('Missing unique');
		if (!entityType) throw new Error('Missing entityType');

		const modalContext = modalManager.open(this, UMB_CREATE_USER_MODAL, {
			data: {
				user: {
					kind,
				},
			},
		});

		modalContext
			?.onSubmit()
			.then(() => {
				this.#requestReloadChildrenOfEntity({ entityType, unique });
			})
			.catch(async () => {
				// modal is closed after creation instead of navigating to the new user.
				// We therefore need to reload the children of the entity
				this.#requestReloadChildrenOfEntity({ entityType, unique });
			});
	}

	async #requestReloadChildrenOfEntity({ entityType, unique }: UmbEntityModel) {
		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType,
			unique,
		});

		eventContext.dispatchEvent(event);
	}

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('user_createUser')}">
				<uui-box>
					<uui-ref-list>
						${map(
							this.#options,
							(item) => html`
								<umb-ref-item
									name=${item.label}
									detail=${item.description}
									icon=${item.icon}
									@click=${(event: Event) => this.#onClick(event, item.kind)}></umb-ref-item>
							`,
						)}
					</uui-ref-list>
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
