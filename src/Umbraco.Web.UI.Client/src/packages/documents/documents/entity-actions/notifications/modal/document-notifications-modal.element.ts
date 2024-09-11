import { UmbDocumentNotificationsRepository } from '../repository/document-notifications.repository.js';
import type {
	UmbDocumentNotificationsModalData,
	UmbDocumentNotificationsModalValue,
} from './document-notifications-modal.token.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { GetDocumentByIdNotificationsResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

interface UmbDocumentNotificationSettings extends GetDocumentByIdNotificationsResponse {}

@customElement('umb-document-notifications-modal')
export class UmbDocumentNotificationsModalElement extends UmbModalBaseElement<
	UmbDocumentNotificationsModalData,
	UmbDocumentNotificationsModalValue
> {
	#unique?: UmbEntityUnique;
	#documentNotificationsRepository = new UmbDocumentNotificationsRepository(this);

	#localizationKeys = [
		{ actionId: 'Umb.Document.Duplicate', key: 'actions_copy' },
		{ actionId: 'Umb.Document.Delete', key: 'actions_delete' },
		{ actionId: 'Umb.Document.Move', key: 'actions_move' },
		{ actionId: 'Umb.Document.Create', key: 'actions_create' },
		{ actionId: 'Umb.Document.PublicAccess', key: 'actions_protect' },
		{ actionId: 'Umb.Document.Publish', key: 'actions_publish' },
		{ actionId: 'Umb.DocumentRecycleBin.Restore', key: 'actions_restore' },
		{ actionId: 'Umb.Document.Permissions', key: 'actions_rights' },
		{ actionId: 'Umb.Document.Rollback', key: 'actions_rollback' },
		{ actionId: 'Umb.Document.Sort', key: 'actions_sort' },
		{ actionId: 'Umb.Document.SendForApproval', key: 'actions_sendtopublish' },
		{ actionId: 'Umb.Document.Update', key: 'actions_update' },
	];

	@state()
	private _settings: UmbDocumentNotificationSettings = [];

	override firstUpdated() {
		this.#unique = this.data?.unique;
		this.#readNotificationSettings();
	}

	async #readNotificationSettings() {
		if (!this.#unique) return;
		const { data } = await this.#documentNotificationsRepository.readNotifications(this.#unique);

		if (!data) return;
		this._settings = data;
	}

	async #updateNotificationSettings() {
		if (!this.#unique) return;

		const subscribedActionIds = this._settings.filter((x) => x.subscribed).map((x) => x.actionId);
		const { error } = await this.#documentNotificationsRepository.updateNotifications(this.#unique, {
			subscribedActionIds,
		});

		if (error) return;
		this._submitModal();
	}

	async #updateSubscription(actionId: string) {
		this._settings = this._settings.map((setting) => {
			if (setting.actionId === actionId) {
				const subscribed = !setting.subscribed;
				return { ...setting, subscribed };
			}
			return setting;
		});
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('notifications_notifications')}>
				<uui-box>
					${repeat(
						this._settings,
						(setting) => setting.actionId,
						(setting) => {
							const localization = this.#localizationKeys.find((x) => x.actionId === setting.actionId);
							return html`<uui-toggle
								id=${setting.actionId}
								@change=${() => this.#updateSubscription(setting.actionId)}
								.label=${localization ? this.localize.term(localization.key) : setting.actionId}
								?checked=${setting.subscribed}></uui-toggle>`;
						},
					)}
				</uui-box>
				<umb-footer-layout slot="footer">
					<uui-button
						slot="actions"
						look="secondary"
						label=${this.localize.term('general_cancel')}
						@click=${this._rejectModal}></uui-button>
					<uui-button
						slot="actions"
						look="primary"
						color="positive"
						label=${this.localize.term('buttons_save')}
						@click=${this.#updateNotificationSettings}></uui-button>
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
