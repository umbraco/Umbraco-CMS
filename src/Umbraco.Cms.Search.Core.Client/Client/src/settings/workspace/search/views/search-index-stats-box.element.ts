import { UMB_SEARCH_WORKSPACE_CONTEXT } from '../search-workspace.context-token.js';
import type { UmbHealthStatusModel } from '../../../types.js';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
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
      <uui-box headline=${this.localize.term('search_indexInfo')}>
        <div class="stats-grid">
          <div class="stat-item">
            <strong><umb-localize key="search_indexAlias">Index Alias</umb-localize></strong>
            <span>${this._indexAlias ?? '—'}</span>
          </div>

          <div class="stat-item">
            <strong><umb-localize key="search_providerName">Provider Name</umb-localize></strong>
            <span>${this._providerName ?? '—'}</span>
          </div>

          <div class="stat-item">
            <strong>
              <umb-localize key="search_tableColumnDocumentCount"> Document Count </umb-localize>
            </strong>
            <span>${this.localize.term('search_documentCount', this._documentCount)}</span>
          </div>

          <div class="stat-item">
            <strong>
              <umb-localize key="search_tableColumnHealthStatus"> Health Status </umb-localize>
            </strong>
            <div>
              <uui-tag look="primary" .color=${this.#getHealthStatusColor(this._healthStatus)}>
                ${this.localize.term('search_healthStatus', this._healthStatus)}
              </uui-tag>
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
    `,
  ];
}

export default UmbSearchIndexStatsBoxElement;

declare global {
  interface HTMLElementTagNameMap {
    'umb-search-index-stats-box': UmbSearchIndexStatsBoxElement;
  }
}
