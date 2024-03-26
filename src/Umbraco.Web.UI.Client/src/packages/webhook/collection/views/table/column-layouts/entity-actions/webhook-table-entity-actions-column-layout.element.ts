import type { UmbWebhookDetailModel } from '../../../../../types.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webhook-table-entity-actions-column-layout')
export class UmbWebhookTableEntityActionsColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value!: UmbWebhookDetailModel;

	@state()
	_isOpen = false;

	#onActionExecuted() {
		this._isOpen = false;
	}

	render() {
		return html`
			<umb-dropdown .open=${this._isOpen} compact hide-expand>
				<uui-symbol-more slot="label"></uui-symbol-more>
				<umb-entity-action-list
					@action-executed=${this.#onActionExecuted}
					entity-type=${this.value.entityType}
					unique=${ifDefined(this.value.unique)}></umb-entity-action-list>
			</umb-dropdown>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-table-entity-actions-column-layout': UmbWebhookTableEntityActionsColumnLayoutElement;
	}
}
