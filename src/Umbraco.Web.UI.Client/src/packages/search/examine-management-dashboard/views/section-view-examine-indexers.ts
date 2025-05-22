import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { HealthStatusResponseModel, IndexResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { HealthStatusModel, IndexerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

import './section-view-examine-searchers.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-dashboard-examine-index')
export class UmbDashboardExamineIndexElement extends UmbLitElement {
	@property()
	indexName!: string;

	@state()
	private _buttonState?: UUIButtonState = undefined;

	@state()
	private _indexData?: IndexResponseModel;

	@state()
	private _loading = true;

	override connectedCallback() {
		super.connectedCallback();
		this.#loadData();
	}

	async #loadData() {
		this._indexData = await this.#getIndexData();

		if (this._indexData?.healthStatus.status === HealthStatusModel.REBUILDING) {
			this._buttonState = 'waiting';
			this._continuousPolling();
		} else {
			this._loading = false;
		}
	}

	async #getIndexData() {
		const { data } = await tryExecute(
			this,
			IndexerService.getIndexerByIndexName({ path: { indexName: this.indexName } }),
		);
		return data;
	}

	private async _continuousPolling() {
		//Checking the server every 5 seconds to see if the index is still rebuilding.
		while (this._buttonState === 'waiting') {
			await new Promise((resolve) => setTimeout(resolve, 5000));
			this._indexData = await this.#getIndexData();
			if (this._indexData?.healthStatus.status !== HealthStatusModel.REBUILDING) {
				this._buttonState = 'success';
			}
		}
		return;
	}

	private async _onRebuildHandler() {
		await umbConfirmModal(this, {
			headline: `${this.localize.term('examineManagement_rebuildIndex')} ${this.indexName}`,
			content: html`<umb-localize key="examineManagement_rebuildIndexWarning"
				>This will cause the index to be rebuilt.<br />
				Depending on how much content there is in your site this could take a while.<br />
				It is not recommended to rebuild an index during times of high website traffic or when editors are editing
				content.</umb-localize
			> `,
			color: 'danger',
			confirmLabel: this.localize.term('examineManagement_rebuildIndex'),
		});

		this._rebuild();
	}
	private async _rebuild() {
		this._buttonState = 'waiting';
		const { error } = await tryExecute(
			this,
			IndexerService.postIndexerByIndexNameRebuild({ path: { indexName: this.indexName } }),
		);
		if (error) {
			this._buttonState = 'failed';
			return;
		}
		this._buttonState = 'success';
		await this.#loadData();
	}

	#renderHealthStatus(healthStatus: HealthStatusResponseModel) {
		const msg = healthStatus.message ? healthStatus.message : healthStatus.status;
		switch (healthStatus.status) {
			case HealthStatusModel.HEALTHY:
				return html`<umb-icon name="icon-check color-green"></umb-icon>${msg}`;
			case HealthStatusModel.CORRUPT:
				return html`<umb-icon name="icon-alert color-red"></umb-icon><div>
					<a href="https://umbra.co/corrupt-indexes" target="_blank"><umb-localize key="examineManagement_corruptStatus">Possible corrupt index detected</umb-localize></a>
					<p><umb-localize key="examineManagement_corruptErrorDescription">Error received when evaluating the index:</umb-localize> </br> </p>${msg}</div>`;
			case HealthStatusModel.UNHEALTHY:
				return html`<umb-icon name="icon-alert color-red"></umb-icon>${msg}`;
			case HealthStatusModel.REBUILDING:
				return html`<umb-icon name="icon-time color-yellow"></umb-icon>${msg}`;
			default:
				return;
		}
	}

	override render() {
		if (!this._indexData || this._loading) return html` <uui-loader-bar></uui-loader-bar>`;

		return html`
			<uui-box headline="${this.indexName}">
				<p>
					<strong><umb-localize key="examineManagement_healthStatus">Health Status</umb-localize></strong
					><br />
					<umb-localize key="examineManagement_healthStatusDescription"
						>The health status of the ${this.indexName} and if it can be read</umb-localize
					>
				</p>
				<div id="health-status">${this.#renderHealthStatus(this._indexData.healthStatus)}</div>
			</uui-box>
			${this.renderIndexSearch()} ${this.renderPropertyList()} ${this.renderTools()}
		`;
	}

	private renderIndexSearch() {
		// Do we want to show the search while rebuilding?
		if (!this._indexData || this._indexData.healthStatus.status === HealthStatusModel.REBUILDING) return nothing;
		return html`<umb-dashboard-examine-searcher .searcherName="${this.indexName}"></umb-dashboard-examine-searcher>`;
	}

	private renderPropertyList() {
		if (!this._indexData) return nothing;

		return html`<uui-box headline=${this.localize.term('examineManagement_indexInfo')}>
			<p>
				<umb-localize key="examineManagement_indexInfoDescription"
					>Lists the properties of the ${this.indexName}</umb-localize
				>
			</p>
			<uui-table class="info">
				<uui-table-row>
					<uui-table-cell style="width:0px; font-weight: bold;">DocumentCount</uui-table-cell>
					<uui-table-cell>${this._indexData.documentCount}</uui-table-cell>
				</uui-table-row>
				<uui-table-row>
					<uui-table-cell style="width:0px; font-weight: bold;">FieldCount</uui-table-cell>
					<uui-table-cell>${this._indexData.fieldCount}</uui-table-cell>
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
		return html` <uui-box headline=${this.localize.term('examineManagement_tools')}>
			<p><umb-localize key="examineManagement_toolsDescription">Tools to manage the ${this.indexName}</umb-localize></p>
			<uui-button
				color="danger"
				look="primary"
				.state="${this._buttonState}"
				@click="${this._onRebuildHandler}"
				.disabled="${this._indexData?.canRebuild ? false : true}"
				label=${this.localize.term('examineManagement_rebuildIndex')}></uui-button>
		</uui-box>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#health-status {
				display: flex;
				align-items: start;
				gap: var(--uui-size-6);
			}

			#health-status umb-icon {
				margin-top: var(--uui-size-1);
			}

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
}

export default UmbDashboardExamineIndexElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-index': UmbDashboardExamineIndexElement;
	}
}
