import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

import { UUIButtonState } from '@umbraco-ui/uui-button';

import { UmbModalService } from '../../../../../core/modal';
import { UmbNotificationService } from '../../../../../core/notification';
import { UmbNotificationDefaultData } from '../../../../../core/notification/layouts/default';

import './section-view-examine-searchers';

import { ApiError, Index, IndexerResource, ProblemDetails } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-dashboard-examine-index')
export class UmbDashboardExamineIndexElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}

			uui-box,
			umb-dashboard-examine-searcher {
				margin-top: var(--uui-size-space-5);
			}

			uui-box p {
				margin-top: 0;
			}

			div.flex {
				display: flex;
			}
			div.flex > uui-button {
				padding-left: var(--uui-size-space-4);
				height: 0;
			}

			uui-input {
				width: 100%;
				margin-bottom: var(--uui-size-space-5);
			}

			uui-table.info uui-table-row:first-child uui-table-cell {
				border-top: none;
			}

			uui-table-head-cell {
				text-transform: capitalize;
			}

			uui-table-row:last-child uui-table-cell {
				padding-bottom: 0;
			}

			uui-icon {
				vertical-align: top;
				padding-right: var(--uui-size-space-5);
			}

			.positive {
				color: var(--uui-color-positive);
			}
			.danger {
				color: var(--uui-color-danger);
			}

			button {
				background: none;
				border: none;
				text-decoration: underline;
				cursor: pointer;
			}
			button.bright {
				font-style: italic;
				color: var(--uui-color-positive-emphasis);
			}
		`,
	];

	@property()
	indexName!: string;

	@state()
	private _buttonState?: UUIButtonState = undefined;

	@state()
	private _indexData?: Index;

	@state()
	private _loading = true;

	private _notificationService?: UmbNotificationService;
	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeAllContexts(['umbNotificationService', 'umbModalService'], (instances) => {
			this._notificationService = instances['umbNotificationService'];
			this._modalService = instances['umbModalService'];
		});
	}

	private async _getIndexData() {
		try {
			this._indexData = await IndexerResource.getIndexerByIndexName({ indexName: this.indexName });
			if (!this._indexData?.isHealthy) {
				this._buttonState = 'waiting';
			}
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Could not fetch index' };
				this._notificationService?.peek('danger', { data });
			}
		}
		this._loading = false;
	}

	async connectedCallback() {
		super.connectedCallback();
		await this._getIndexData();
	}

	private async _onRebuildHandler() {
		const modalHandler = this._modalService?.confirm({
			headline: `Rebuild ${this.indexName}`,
			content: html`
				This will cause the index to be rebuilt.<br />
				Depending on how much content there is in your site this could take a while.<br />
				It is not recommended to rebuild an index during times of high website traffic or when editors are editing
				content.
			`,
			color: 'danger',
			confirmLabel: 'Rebuild',
		});
		modalHandler?.onClose().then(({ confirmed }) => {
			if (confirmed) this._rebuild();
		});
	}
	private async _rebuild() {
		this._buttonState = 'waiting';
		try {
			await IndexerResource.postIndexerByIndexNameRebuild({ indexName: this.indexName });
			this._buttonState = 'success';
			await this._getIndexData();
		} catch (e) {
			this._buttonState = 'failed';
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Rebuild error' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	render() {
		if (!this._indexData || this._loading) return html` <uui-loader-bar></uui-loader-bar>`;

		return html`
			<uui-box headline="${this.indexName}">
				<p>
					<strong>Health Status</strong><br />
					The health status of the ${this.indexName} and if it can be read
				</p>
				<div>
					<uui-icon-essentials>
						<uui-icon
							name=${this._indexData.isHealthy ? `check` : `wrong`}
							class=${this._indexData.isHealthy ? 'positive' : 'danger'}>
						</uui-icon>
					</uui-icon-essentials>
					${this._indexData.healthStatus}
				</div>
			</uui-box>
			${this.renderIndexSearch()} ${this.renderPropertyList()} ${this.renderTools()}
		`;
	}

	private renderIndexSearch() {
		if (!this._indexData || !this._indexData.isHealthy) return nothing;
		return html`<umb-dashboard-examine-searcher searcherName="${this.indexName}"></umb-dashboard-examine-searcher>`;
	}

	private renderPropertyList() {
		if (!this._indexData) return nothing;

		return html`<uui-box headline="Index info">
			<p>Lists the properties of the ${this.indexName}</p>
			<uui-table class="info">
				<uui-table-row>
					<uui-table-cell style="width:0px; font-weight: bold;"> documentCount </uui-table-cell>
					<uui-table-cell>${this._indexData.documentCount} </uui-table-cell>
				</uui-table-row>
				<uui-table-row>
					<uui-table-cell style="width:0px; font-weight: bold;"> fieldCount </uui-table-cell>
					<uui-table-cell>${this._indexData.fieldCount} </uui-table-cell>
				</uui-table-row>
				${this._indexData.providerProperties
					? Object.entries(this._indexData.providerProperties).map((entry) => {
							return html`<uui-table-row>
								<uui-table-cell style="width:0; font-weight: bold;"> ${entry[0]} </uui-table-cell>
								<uui-table-cell clip-text> ${entry[1]} </uui-table-cell>
							</uui-table-row>`;
					  })
					: ''}
			</uui-table>
		</uui-box>`;
	}

	private renderTools() {
		return html` <uui-box headline="Tools">
			<p>Tools to manage the ${this.indexName}</p>
			<uui-button
				color="danger"
				look="primary"
				.state="${this._buttonState}"
				@click="${this._onRebuildHandler}"
				.disabled="${!this._indexData?.canRebuild ?? true}"
				label="Rebuild index">
				Rebuild
			</uui-button>
		</uui-box>`;
	}
}

export default UmbDashboardExamineIndexElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-index': UmbDashboardExamineIndexElement;
	}
}
