import {
	html,
	LitElement,
	customElement,
	property,
	ifDefined,
	nothing,
	css,
	styleMap,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbNotificationDefaultData, UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';

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
				<div id="message" style=${styleMap({ whiteSpace: this.data.whitespace })}>${this.data.message}</div>
				${this.#renderStructuredList(this.data.structuredList)}
			</uui-toast-notification-layout>
		`;
	}

	#renderStructuredList(list: unknown) {
		if (!this.data.structuredList) return nothing;
		if (typeof list !== 'object' || list === null) return nothing;

		return html`${Object.entries(list).map(
			([property, errors]) =>
				html`<div class="structured-list">
					<p>${property}:</p>
					<ul>
						${this.#renderListItem(errors)}
					</ul>
				</div>`,
		)}`;
	}

	#renderListItem(items: unknown) {
		if (Array.isArray(items)) {
			return items.map((item) => html`<li>${item}</li>`);
		} else {
			return html`<li>${items}</li>`;
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			.structured-list ul {
				margin: 0;
			}
			.structured-list p {
				margin: var(--uui-size-3) 0 var(--uui-size-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-notification-layout-default': UmbNotificationLayoutDefaultElement;
	}
}
