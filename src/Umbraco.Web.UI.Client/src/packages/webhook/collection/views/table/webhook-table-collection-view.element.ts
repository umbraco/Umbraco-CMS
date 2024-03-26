import type { UmbWebhookDetailModel } from '../../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './column-layouts/boolean/webhook-table-boolean-column-layout.element.js';
import './column-layouts/name/webhook-table-name-column-layout.element.js';
import './column-layouts/entity-actions/webhook-table-entity-actions-column-layout.element.js';

@customElement('umb-webhook-table-collection-view')
export class UmbWebhookTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Enabled',
			alias: 'enabled',
			elementName: 'umb-webhook-table-boolean-column-layout',
		},
		{
			name: 'URL',
			alias: 'url',
		},
		{
			name: 'Events',
			alias: 'events',
		},
		{
			name: 'Types',
			alias: 'types',
		},
		{
			name: '',
			alias: 'entityActions',
			elementName: 'umb-webhook-table-entity-actions-column-layout',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbWebhookDetailModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
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
			return {
				id: webhook.unique,
				icon: 'icon-webhook',
				data: [
					{
						columnAlias: 'enabled',
						value: webhook.enabled,
					},
					{
						columnAlias: 'url',
						value: webhook.url,
					},
					{
						columnAlias: 'events',
						value: webhook.events,
					},
					{
						columnAlias: 'types',
						value: webhook.types,
					},
					{
						columnAlias: 'entityActions',
						value: webhook,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static styles = [
		UmbTextStyles,
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
