import { html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { UmbPropertyAction } from '../../shared/property-action/property-action.model.js';
import {
	UmbNotificationDefaultData,
	UmbNotificationContext,
	UMB_NOTIFICATION_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/notification';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-property-action-copy')
export class UmbPropertyActionCopyElement extends UmbLitElement implements UmbPropertyAction {
	@property()
	value = '';

	private _notificationContext?: UmbNotificationContext;

	constructor() {
		super();

		// TODO implement a property context
		this.consumeContext('umbProperty', (property) => {
			console.log('PROPERTY', property);
		});

		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this._notificationContext = instance;
		});
	}

	private _handleLabelClick() {
		const data: UmbNotificationDefaultData = { message: 'Copied to clipboard' };
		this._notificationContext?.peek('positive', { data });
		// TODO: how do we want to close the menu? Testing an event based approach
		this.dispatchEvent(new CustomEvent('close', { bubbles: true, composed: true }));
	}

	render() {
		return html` <uui-menu-item label="Copy" @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name="copy"></uui-icon>
		</uui-menu-item>`;
	}
}

export default UmbPropertyActionCopyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-action-copy': UmbPropertyActionCopyElement;
	}
}
