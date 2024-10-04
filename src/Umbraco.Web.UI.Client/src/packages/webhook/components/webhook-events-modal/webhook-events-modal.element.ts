import { UmbWebhookEventRepository } from '../../repository/event/webhook-event.repository.js';
import type { UmbWebhookEventModel } from '../../types.js';
import type { UmbWebhookPickerModalData, UmbWebhookPickerModalValue } from './webhook-events-modal.token.js';
import { customElement, html, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

@customElement('umb-webhook-events-modal')
export class UmbWebhookEventsModalElement extends UmbModalBaseElement<
	UmbWebhookPickerModalData,
	UmbWebhookPickerModalValue
> {
	@state()
	_events: Array<UmbWebhookEventModel> = [];

	#repository = new UmbWebhookEventRepository(this);

	#selectionManager = new UmbSelectionManager(this);

	#eventsRequest?: Promise<any>;

	override connectedCallback(): void {
		super.connectedCallback();

		this.#selectionManager.setSelectable(true);
		this.#selectionManager.setMultiple(true);
		this.#selectionManager.setSelection(this.data?.events.map((item) => item.alias) ?? []);

		this.#requestEvents();
		this.#observeSelection();
	}

	async #observeSelection() {
		await this.#eventsRequest;

		this.observe(this.#selectionManager.selection, (selection) => {
			this.value = { events: this._events.filter((item) => selection.includes(item.alias)) };
		});
	}

	async #requestEvents() {
		this.#eventsRequest = this.#repository.requestEvents();
		const { data } = await this.#eventsRequest;

		if (!data) return;

		this._events = data.items;
	}

	#submit() {
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	#getItemDisabled(item: UmbWebhookEventModel) {
		const selection = this.#selectionManager.getSelection();

		if (!selection.length) return false;

		const selectedEvents = this._events.filter((item) => selection.includes(item.alias));
		return selectedEvents[0].eventType !== item.eventType;
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('webhooks_selectEvents')}>
				<uui-box>
					${repeat(
						this._events,
						(item) => item.alias,
						(item) => html`
							<uui-menu-item
								label=${item.eventName}
								?disabled=${this.#getItemDisabled(item)}
								selectable
								@selected=${() => this.#selectionManager.select(item.alias)}
								@deselected=${() => this.#selectionManager.deselect(item.alias)}
								?selected=${this.value.events.includes(item)}>
								<uui-icon slot="icon" name="icon-globe"></uui-icon>
							</uui-menu-item>
						`,
					)}
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_cancel')} @click=${this.#close}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this.#submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}
}

export default UmbWebhookEventsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-events-modal': UmbWebhookEventsModalElement;
	}
}
