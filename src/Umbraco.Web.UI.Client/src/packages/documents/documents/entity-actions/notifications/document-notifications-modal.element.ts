import type {
	UmbDocumentNotificationsModalData,
	UmbDocumentNotificationsModalValue,
} from './document-notifications-modal.token.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-document-notifications-modal')
export class UmbDocumentNotificationsModalElement extends UmbModalBaseElement<
	UmbDocumentNotificationsModalData,
	UmbDocumentNotificationsModalValue
> {
	#unique?: UmbEntityUnique;

	constructor() {
		super();
		this.#unique = this.data?.unique;
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('notifications_notifications')}>
				<uui-box>
					${this.#unique}
					<uui-toggle label="Copy"></uui-toggle>
					<uui-toggle label="Delete"></uui-toggle>
					<uui-toggle label="Move"></uui-toggle>
					<uui-toggle label="Create"></uui-toggle>
					<uui-toggle label="Restrict Public Access"></uui-toggle>
					<uui-toggle label="Publish"></uui-toggle>
					<uui-toggle label="Restore"></uui-toggle>
					<uui-toggle label="Permissions"></uui-toggle>
					<uui-toggle label="Rollback"></uui-toggle>
					<uui-toggle label="Sort"></uui-toggle>
					<uui-toggle label="Send To Publish"></uui-toggle>
					<uui-toggle label="Update"></uui-toggle>
				</uui-box>
				<umb-footer-layout slot="footer">
					<uui-button slot="actions" look="secondary" label=${this.localize.term('general_cancel')}></uui-button>
					<uui-button slot="actions" look="primary" label=${this.localize.term('actions_rollback')}></uui-button>
				</umb-footer-layout>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-toggle {
				display: block;
			}
		`,
	];
}

export default UmbDocumentNotificationsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-notifications-modal': UmbDocumentNotificationsModalElement;
	}
}
