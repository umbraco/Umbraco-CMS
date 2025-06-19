import type { UmbWebhookDeliveryDetailModel } from '../../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';

import './column-layouts/status-code/webhook-delivery-table-status-code-column-layout.element.js';

@customElement('umb-webhook-delivery-table-collection-view')
export class UmbWebhookDeliveryTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_date'),
			alias: 'date',
		},
		{
			name: this.localize.term('webhooks_url'),
			alias: 'eventUrl',
		},
		{
			name: this.localize.term('webhooks_event'),
			alias: 'eventAlias',
		},
		{
			name: this.localize.term('webhooks_statusCode'),
			alias: 'statusCode',
			elementName: 'umb-webhook-delivery-table-status-code-column-layout',
		},
		{
			name: this.localize.term('webhooks_retryCount'),
			alias: 'retryCount',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbWebhookDeliveryDetailModel>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(deliveries: Array<UmbWebhookDeliveryDetailModel>) {
		this._tableItems = deliveries.map((delivery) => {
			return {
				id: delivery.unique,
				icon: 'icon-box-alt',
				data: [
					{
						columnAlias: 'date',
						value: html`<umb-date-table-column-view .value=${delivery.date}></umb-date-table-column-view>`,
					},
					{
						columnAlias: 'eventUrl',
						value: delivery.url,
					},
					{
						columnAlias: 'eventAlias',
						value: delivery.eventAlias,
					},
					{
						columnAlias: 'statusCode',
						value: delivery.statusCode,
					},
					{
						columnAlias: 'retryCount',
						value: delivery.retryCount,
					},
				],
			};
		});
	}

	override render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbWebhookDeliveryTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-delivery-table-collection-view': UmbWebhookDeliveryTableCollectionViewElement;
	}
}
