import { UMB_SEARCH_WORKSPACE_CONTEXT } from '../workspace/search-workspace.context-token.js';
import type { UmbHealthStatusModel, UmbSearchIndexState } from '../types.js';
import { html, customElement, state, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-search-index-stats-box')
export class UmbSearchIndexStatsBoxElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_SEARCH_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _indexAlias?: string;

	@state()
	private _providerName?: string;

	@state()
	private _documentCount?: number;

	@state()
	private _healthStatus?: UmbHealthStatusModel;

	@state()
	private _state?: UmbSearchIndexState;

	constructor() {
		super();

		this.consumeContext(UMB_SEARCH_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context;
			this.#observeData();
		});
	}

	#observeData() {
		this.observe(
			this.#workspaceContext?.name,
			(name) => {
				this._indexAlias = name;
			},
			'_observeName',
		);

		this.observe(
			this.#workspaceContext?.providerName,
			(name) => {
				this._providerName = name;
			},
			'_observeProviderName',
		);

		this.observe(
			this.#workspaceContext?.documentCount,
			(count) => {
				this._documentCount = count;
			},
			'_observeDocumentCount',
		);

		this.observe(
			this.#workspaceContext?.healthStatus,
			(status) => {
				this._healthStatus = status;
			},
			'_observeHealthStatus',
		);

		this.observe(
			this.#workspaceContext?.state,
			(state) => {
				this._state = state;
			},
			'_observeState',
		);
	}

	/**
	 * A rebuild the user just started is not reported by the server until the next load.
	 * @returns {UmbHealthStatusModel | undefined} The status to display for the index.
	 */
	#effectiveHealthStatus(): UmbHealthStatusModel | undefined {
		return this._state === 'loading' ? 'Rebuilding' : this._healthStatus;
	}

	#getHealthStatusColor(status?: UmbHealthStatusModel): string {
		switch (status) {
			case 'Healthy':
				return 'positive';
			case 'Rebuilding':
			case 'Empty':
				return 'warning';
			case 'Corrupted':
				return 'danger';
			default:
				return 'default';
		}
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('searchManagement_indexInfo')}>
				<div class="stats-grid">
					<div class="stat-item">
						<strong><umb-localize key="searchManagement_indexAlias">Alias</umb-localize></strong>
						<span>${this._indexAlias ?? '—'}</span>
					</div>

					<div class="stat-item">
						<strong><umb-localize key="searchManagement_providerName">Provider</umb-localize></strong>
						<span>${this._providerName ?? '—'}</span>
					</div>

					<div class="stat-item">
						<strong>
							<umb-localize key="searchManagement_tableColumnDocumentCount">Document count</umb-localize>
						</strong>
						<span>${this.localize.term('searchManagement_documentCount', this._documentCount ?? 0)}</span>
					</div>

					<div class="stat-item">
						<strong>
							<umb-localize key="searchManagement_tableColumnHealthStatus">Health status</umb-localize>
						</strong>
						<div class="health-status">
							<uui-tag look="secondary" .color=${this.#getHealthStatusColor(this.#effectiveHealthStatus())}>
								${this.localize.term('searchManagement_healthStatus', this.#effectiveHealthStatus() ?? 'Unknown')}
							</uui-tag>
							${this._state === 'loading' ? html`<uui-loader-circle></uui-loader-circle>` : nothing}
						</div>
					</div>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}

			.stats-grid {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
			}

			.stat-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			.health-status {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbSearchIndexStatsBoxElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-index-stats-box': UmbSearchIndexStatsBoxElement;
	}
}
