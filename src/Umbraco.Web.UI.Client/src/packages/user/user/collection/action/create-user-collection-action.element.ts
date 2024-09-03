import { UMB_CREATE_USER_MODAL } from '../../modals/create/create-user-modal.token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';

const elementName = 'umb-create-user-collection-action-button';
@customElement(elementName)
export class UmbCollectionActionButtonElement extends UmbLitElement {
	@state()
	private _popoverOpen = false;

	async #onClick(event: Event) {
		event.stopPropagation();
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);

		const unique = entityContext.getUnique();
		const entityType = entityContext.getEntityType();

		if (unique === undefined) throw new Error('Missing unique');
		if (!entityType) throw new Error('Missing entityType');

		const modalContext = modalManager.open(this, UMB_CREATE_USER_MODAL);
		modalContext?.onSubmit().catch(async () => {
			// modal is closed after creation instead of navigating to the new user.
			// We therefore need to reload the children of the entity
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType,
				unique,
			});

			eventContext.dispatchEvent(event);
		});
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		const label = this.localize.term('general_create');

		return html`
			<uui-button popovertarget="collection-action-menu-popover" label=${label} color="default" look="outline">
				${label}
				<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container
				id="collection-action-menu-popover"
				placement="bottom-start"
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						<uui-menu-item label="Default" @click=${this.#onClick}>
							<umb-icon slot="icon" name="icon-user"></umb-icon>
						</uui-menu-item>
						<uui-menu-item label="Api" @click=${this.#onClick}>
							<umb-icon slot="icon" name="icon-binarycode"></umb-icon>
						</uui-menu-item>
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}
}

export { UmbCollectionActionButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCollectionActionButtonElement;
	}
}
