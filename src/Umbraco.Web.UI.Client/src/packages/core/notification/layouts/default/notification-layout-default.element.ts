import { html, LitElement, customElement, property, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import type { UmbNotificationDefaultData, UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';

export type { UmbNotificationDefaultData };

@customElement('umb-notification-layout-default')
export class UmbNotificationLayoutDefaultElement extends LitElement {
	@property({ attribute: false })
	notificationHandler!: UmbNotificationHandler;

	@property({ type: Object })
	data!: UmbNotificationDefaultData;

	render() {
		return html`
			<uui-toast-notification-layout id="layout" headline="${ifDefined(this.data.headline)}" class="uui-text">
				<div id="message">${this.data.message}</div>
			</uui-toast-notification-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-notification-layout-default': UmbNotificationLayoutDefaultElement;
	}
}
