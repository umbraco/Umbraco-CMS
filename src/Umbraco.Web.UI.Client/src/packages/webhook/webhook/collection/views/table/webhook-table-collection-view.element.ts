import type { UmbWebhookDetailModel } from '../../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';

import './column-layouts/name/webhook-table-name-column-layout.element.js';
import './column-layouts/content-type/webhook-table-name-column-layout.element.js';
import { UMB_EDIT_WEBHOOK_WORKSPACE_PATH_PATTERN } from '../../../paths.js';

@customElement('umb-webhook-table-collection-view')
export class UmbWebhookTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: this.localize.term('general_name'),
			alias: 'name',
		},
		{
			name: this.localize.term('webhooks_url'),
			alias: 'url',
		},
		{
			name: this.localize.term('webhooks_events'),
			alias: 'events',
		},
		{
			name: this.localize.term('webhooks_types'),
			alias: 'contentTypes',
			elementName: 'umb-webhook-table-content-type-column-layout',
		},
		{
			name: this.localize.term('general_status'),
			alias: 'status',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbWebhookDetailModel>;
	#enabledLabel: string;
	#disabledLabel: string;
	#unnamedWebhookLabel: string;

	constructor() {
		super();

		this.#enabledLabel = this.localize.term('webhooks_enabled');
		this.#disabledLabel = this.localize.term('webhooks_disabled');
		this.#unnamedWebhookLabel = this.localize.term('webhooks_unnamedWebhook');

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(webhooks: Array<UmbWebhookDetailModel>) {
		this._tableItems = webhooks.map((webhook) => {
			const name = webhook.name || `(${this.#unnamedWebhookLabel})`;
			const path = UMB_EDIT_WEBHOOK_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: webhook.unique });

			return {
				id: webhook.unique,
				icon: 'icon-webhook',
				data: [
					{
						columnAlias: 'name',
						value: html`<a href=${path}>${name}</a>`,
					},
					{
						columnAlias: 'url',
						value: webhook.url,
					},
					{
						columnAlias: 'events',
						value: webhook.events.map((event) => event.eventName).join(', ') || 'None',
					},
					{
						columnAlias: 'contentTypes',
						value: { contentTypeName: webhook.events[0].eventType, contentTypes: webhook.contentTypes },
					},
					{
						columnAlias: 'status',
						value: html`<uui-tag color=${webhook.enabled ? 'positive' : 'danger'}
							>${webhook.enabled ? this.#enabledLabel : this.#disabledLabel}</uui-tag
						>`,
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: webhook.entityType,
								unique: webhook.unique,
								name: webhook.name,
							}}></umb-entity-actions-table-column-view>`,
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

export default UmbWebhookTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-table-collection-view': UmbWebhookTableCollectionViewElement;
	}
}
