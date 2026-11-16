import type { UmbNotificationDefaultData } from '../../types.js';
import type { UmbNotificationHandler } from '../../notification-handler.js';
import {
	html,
	LitElement,
	customElement,
	property,
	ifDefined,
	css,
	unsafeHTML,
} from '@umbraco-cms/backoffice/external/lit';
import { sanitizeHTML } from '@umbraco-cms/backoffice/utils';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

export type { UmbNotificationDefaultData };

@customElement('umb-notification-layout-default')
export class UmbNotificationLayoutDefaultElement extends LitElement {
	@property({ attribute: false })
	notificationHandler!: UmbNotificationHandler;

	@property({ type: Object })
	data!: UmbNotificationDefaultData;

	override render() {
		return html`
			<uui-toast-notification-layout id="layout" headline="${ifDefined(this.data.headline)}" class="uui-text">
				<div id="message">${this.#renderMessage()}</div>
			</uui-toast-notification-layout>
		`;
	}

	#renderMessage() {
		const htmlMessage = this.data.htmlMessage;
		if (htmlMessage) {
			return typeof htmlMessage === 'string' ? unsafeHTML(sanitizeHTML(htmlMessage)) : htmlMessage;
		}
		return this.data.message;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#message {
				white-space: pre-line;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-notification-layout-default': UmbNotificationLayoutDefaultElement;
	}
}
