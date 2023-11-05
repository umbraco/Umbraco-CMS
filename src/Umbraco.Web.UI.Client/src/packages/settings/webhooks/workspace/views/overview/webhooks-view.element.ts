import { UmbWebhookRepository } from '../../../repository/webhooks.repository.js';
import { UmbWebhooksWorkspaceContext, UMB_APP_WEBHOOKS_CONTEXT_TOKEN } from '../../webhooks.context.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-webhooks-overview-view')
export class UmbWebhooksOverviewViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Enabled',
			alias: 'enabled',
			elementName: 'umb-language-root-table-name-column-layout',
		},
		{
			name: 'Events',
			alias: 'events',
		},
		{
			name: 'Url',
			alias: 'url',
		},
		{
			name: 'Types',
			alias: 'types',
		},
		{
			name: '',
			alias: 'delete',
			elementName: 'umb-language-root-table-delete-column-layout',
		},
	];

	@state()s
	private _tableItems: Array<UmbTableItem> = [];

	#webhookRepository = new UmbWebhookRepository(this);
	
	#webhooksContext?: UmbWebhooksWorkspaceContext;
	constructor() {
		super();

		this.consumeContext(UMB_APP_WEBHOOKS_CONTEXT_TOKEN, (instance) => {
			this.#webhooksContext = instance;
		});
	}

	connectedCallback() {
		super.connectedCallback();
		this.#observeWebhook();
	}

	async #observeWebhook() {
		const { asObservable } = await this.#webhookRepository.requestWebhooks();

		if (asObservable) {
			this.observe(asObservable(), (webhook) => this.#createTableItems(webhooks));
		}
	}

	#createTableItems(webhooks: Array<LanguageResponseModel>) {
		this._tableItems = webhooks.map((webhook) => {
			return {
				id: webhook.id ?? '',
				icon: 'umb:anchor',
				data: [
					{
						columnAlias: 'enabled',
						value: webhook.enabled,
					},
					{
						columnAlias: 'events',
						value: webhook.events,
					},
					{
						columnAlias: 'url',
						value: webhook.url,
					},
					{
						columnAlias: 'types',
						value: webhook.types,
					},
					{
						columnAlias: 'delete',
						value: webhook,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-body-layout main-no-padding headline="Languages">
				<umb-body-layout header-transparent>
					<uui-button
						slot="header"
						label="Add webhook"
						look="outline"
						color="default"
						href="section/settings/workspace/webhook/create">
					</uui-button>

					<!--- TODO: investigate if it's possible to use a collection component here --->
					<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
				</umb-body-layout>
			</umb-body-layout>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhooks-overview-view': UmbWebhooksOverviewViewElement;
	}
}
