import type { UmbSearchIndex } from '../types.js';
import { UMB_SEARCH_INDEX_ENTITY_TYPE } from '@umbraco-cms/search/global';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { customElement, html, state, when } from '@umbraco-cms/backoffice/external/lit';
import {
  UMB_COLLECTION_CONTEXT,
  type UmbDefaultCollectionContext,
} from '@umbraco-cms/backoffice/collection';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
  UmbTableColumn,
  UmbTableConfig,
  UmbTableItem,
} from '@umbraco-cms/backoffice/components';

@customElement('umb-search-root-collection-view')
export default class UmbSearchRootCollectionView extends UmbLitElement {
  @state()
  private _tableItems: Array<UmbTableItem> = [];

  private _tableConfig: UmbTableConfig = {
    allowSelection: false,
  };

  private _tableColumns: Array<UmbTableColumn> = [
    {
      name: this.localize.term('search_tableColumnAlias'),
      alias: 'indexAlias',
    },
    {
      name: this.localize.term('search_tableColumnHealthStatus'),
      alias: 'healthStatus',
    },
    {
      name: this.localize.term('search_tableColumnDocumentCount'),
      alias: 'documentCount',
    },
    {
      name: '',
      alias: 'entityActions',
      align: 'right',
    },
  ];

  #collectionContext?: UmbDefaultCollectionContext<UmbSearchIndex, never>;

  constructor() {
    super();

    this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
      this.#collectionContext = instance;
      this.#observeCollectionItems();
    });
  }

  override render() {
    return html`
      <umb-table
        .config=${this._tableConfig}
        .columns=${this._tableColumns}
        .items=${this._tableItems}
      ></umb-table>
    `;
  }

  #observeCollectionItems() {
    this.observe(
      this.#collectionContext?.items,
      (items: UmbSearchIndex[]) => {
        // Make sure we are connected to the DOM, otherwise we might update state when not needed
        // or when changing to another workspace with similar context.
        if (!this.isConnected) return;

        this.#createTable(items);
      },
      '_itemsObserver',
    );
  }

  #createTable(items: UmbSearchIndex[]) {
    this._tableItems = items?.map((item) => {
      return {
        id: item.unique,
        icon: this.#healthStatusIcon(item),
        data: [
          {
            columnAlias: 'indexAlias',
            value: html`<a
              href=${`section/settings/workspace/${UMB_SEARCH_INDEX_ENTITY_TYPE}/edit/${item.unique}`}
              >${item.unique}</a
            >`,
          },
          {
            columnAlias: 'healthStatus',
            value: when(
              item.state === 'loading',
              () => html`<uui-loader-bar></uui-loader-bar>`,
              () => this.localize.term('search_healthStatus', item.healthStatus),
            ),
          },
          {
            columnAlias: 'documentCount',
            value: this.localize.term(
              'search_documentCount',
              this.localize.number(item.documentCount),
            ),
          },
          {
            columnAlias: 'entityActions',
            value: html`<umb-entity-actions-table-column-view
              .value=${{
                entityType: item.entityType,
                unique: item.unique,
                name: item.unique,
              }}
            ></umb-entity-actions-table-column-view>`,
          },
        ],
      };
    });
  }

  #healthStatusIcon(item: UmbSearchIndex) {
    if (item.state === 'loading') {
      return 'icon-loading color-blue';
    }
    switch (item.healthStatus) {
      case 'Healthy':
        return 'icon-check color-green';
      case 'Rebuilding':
        return 'icon-time color-yellow';
      case 'Empty':
        return 'icon-check color-yellow';
      default:
        // Corrupted or any other status
        return 'icon-alert color-red';
    }
  }

  static override readonly styles = [UmbTextStyles];
}
