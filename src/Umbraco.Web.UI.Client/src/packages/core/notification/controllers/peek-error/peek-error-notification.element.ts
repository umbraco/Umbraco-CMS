import { customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbNotificationHandler } from '../../notification-handler.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbPeekErrorArgs } from '../../types.js';
import { UMB_ERROR_VIEWER_MODAL } from '../../index.js';

@customElement('umb-peek-error-notification')
export class UmbPeekErrorNotificationElement extends UmbLitElement {
	@property({ attribute: false })
	public data?: UmbPeekErrorArgs;

	public notificationHandler!: UmbNotificationHandler;

	async #onClick() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);

		modalManager.open(this, UMB_ERROR_VIEWER_MODAL, { data: this.data?.details });

		this.notificationHandler.close();
	}

	protected override render() {
		return this.data
			? html`<uui-toast-notification-layout headline=${ifDefined(this.data.headline)}
					>${this.data.message}${this.data.details
						? html`<uui-button
								slot="actions"
								look="primary"
								color="danger"
								label=${this.localize.term('defaultdialogs_seeErrorAction')}
								@click=${this.#onClick}></uui-button>`
						: nothing}</uui-toast-notification-layout
				>`
			: nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-peek-error-notification': UmbPeekErrorNotificationElement;
	}
}
