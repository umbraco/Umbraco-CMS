import { UmbSearchQueryRepository } from '../../../repositories/search-query.repository.js';
import type { UmbSearchRequest, UmbSearchResult, UmbHealthStatusModel } from '../../../types.js';
import { UMB_SEARCH_WORKSPACE_CONTEXT } from '../search-workspace.context-token.js';

import {
  css,
  customElement,
  html,
  nothing,
  state,
  when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { debounce, UmbPaginationManager } from '@umbraco-cms/backoffice/utils';
import type {
  UmbTableColumn,
  UmbTableConfig,
  UmbTableItem,
} from '@umbraco-cms/backoffice/components';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UMB_SEARCH_DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/search/global';
import type {
  UUIInputElement,
  UUIPaginationElement,
  UUISelectElement,
} from '@umbraco-cms/backoffice/external/uui';

const PAGE_SIZE = 10;

@customElement('umb-search-index-search-box')
export class UmbSearchIndexSearchBoxElement extends UmbLitElement {
  #workspaceContext?: typeof UMB_SEARCH_WORKSPACE_CONTEXT.TYPE;
  #queryRepository = new UmbSearchQueryRepository(this);
  #inputValue = ''; // Non-reactive property for input value
  #routeBuilder?: (params: { entityType: string }) => string; // Route builder function
  #debouncedSearch = debounce(() => {
    void this.#handleSearch();
  }, 300);

  #pagination = new UmbPaginationManager();
  #initialPage?: number;
  #urlCulture?: string; // Temporary storage for culture from URL params before workspace context connects
  #defaultAppCulture?: string; // App default culture from UMB_APP_LANGUAGE_CONTEXT

  private _tableConfig: UmbTableConfig = {
    allowSelection: false,
  };

  private _tableColumns: Array<UmbTableColumn> = [
    {
      name: this.localize.term('search_tableColumnName'),
      alias: 'name',
    },
    {
      name: this.localize.term('search_tableColumnEntityType'),
      alias: 'entityType',
    },
    {
      name: '',
      alias: 'actions',
      align: 'right',
    },
  ];

  @state()
  private _tableItems: Array<UmbTableItem> = [];

  @state()
  private _indexAlias?: string; // driven by #indexAlias state, kept for template reactivity

  @state()
  private _healthStatus?: UmbHealthStatusModel;

  @state()
  private _searchQuery = '';

  @state()
  private _searchResults?: UmbSearchResult;

  @state()
  private _isSearching = false;

  @state()
  private _error?: string;

  @state()
  private _searchStatusMessage = ''; // For screen reader announcements

  @state()
  private _currentPage = 1;

  @state()
  private _totalPages = 1;

  @state()
  private _languages: Array<UmbLanguageDetailModel> = [];

  @state()
  private _selectedCulture?: string;

  @state()
  private _hasMultipleLanguages = false;

  constructor() {
    super();

    // Read URL params first, before any observers or context consumers fire
    this.#readUrlParams();

    this.#pagination.setPageSize(PAGE_SIZE);

    this.observe(
      this.#pagination.currentPage,
      (page) => {
        this._currentPage = page;
      },
      '_observeCurrentPage',
    );

    this.observe(
      this.#pagination.totalPages,
      (totalPages) => {
        this._totalPages = totalPages;
      },
      '_observeTotalPages',
    );

    // Register modal route for opening entities
    new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
      .addAdditionalPath(':entityType')
      .onSetup((routingInfo) => {
        return {
          data: {
            entityType: routingInfo.entityType,
            preset: {},
          },
        };
      })
      .observeRouteBuilder((routeBuilder) => {
        // Store the route builder function to call dynamically per entity type
        this.#routeBuilder = routeBuilder;
      });

    this.consumeContext(UMB_SEARCH_WORKSPACE_CONTEXT, (workspaceContext) => {
      if (!workspaceContext) return;
      this.#workspaceContext = workspaceContext;

      // Seed culture from URL params or app default (URL takes precedence)
      const initialCulture = this.#urlCulture ?? this.#defaultAppCulture;
      if (initialCulture && !workspaceContext.getSelectedCulture()) {
        workspaceContext.setSelectedCulture(initialCulture);
      }

      // Trigger search when both alias and culture are ready on the workspace context
      this.observe(
        observeMultiple([workspaceContext.name, workspaceContext.selectedCulture]),
        ([alias, culture]) => {
          this._indexAlias = alias ?? undefined;
          this._selectedCulture = culture;
          if (alias && culture) {
            void this.#handleSearch();
          }
        },
        '_observeSearchReady',
      );

      this.#observeHealthStatus();
    });

    this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (languageContext) => {
      if (!languageContext) return;

      this.observe(
        languageContext.languages,
        (languages) => {
          this._languages = languages;
          this._hasMultipleLanguages = languages.length > 1;
        },
        '_observeLanguages',
      );

      this.observe(
        languageContext.appLanguageCulture,
        (culture) => {
          this.#defaultAppCulture = culture;
          // If workspace context is ready and no culture set yet, apply default
          if (culture && !this.#workspaceContext?.getSelectedCulture()) {
            this.#workspaceContext?.setSelectedCulture(culture);
          }
        },
        '_observeAppLanguageCulture',
      );
    });
  }

  #readUrlParams() {
    const url = new URL(window.location.href);
    const query = url.searchParams.get('query');
    const page = url.searchParams.get('page');
    const culture = url.searchParams.get('culture');

    if (culture) {
      this.#urlCulture = culture;
    }

    if (query) {
      this.#inputValue = query;
      this._searchQuery = query;
    }

    if (page) {
      const pageNumber = parseInt(page, 10);
      if (!isNaN(pageNumber) && pageNumber >= 1) {
        this.#initialPage = pageNumber;
      }
    }
  }

  #updateUrlParams() {
    const url = new URL(window.location.href);

    if (this._searchQuery.trim()) {
      url.searchParams.set('query', this._searchQuery);
    } else {
      url.searchParams.delete('query');
    }

    const currentPage = this.#pagination.getCurrentPageNumber();
    if (currentPage > 1) {
      url.searchParams.set('page', String(currentPage));
    } else {
      url.searchParams.delete('page');
    }

    if (this._selectedCulture) {
      url.searchParams.set('culture', this._selectedCulture);
    } else {
      url.searchParams.delete('culture');
    }

    history.replaceState(null, '', url.toString());
  }

  #observeHealthStatus() {
    this.observe(
      this.#workspaceContext?.healthStatus,
      (status) => {
        this._healthStatus = status;
      },
      '_observeHealthStatus',
    );
  }

  get #isSearchDisabled(): boolean {
    return this._healthStatus !== 'Healthy';
  }

  override render() {
    return html`
      <uui-box headline=${this.localize.term('search_searchBox')}>
        <div
          class="search-container"
          role="search"
          aria-label=${this.localize.term('search_searchFormLabel', this._indexAlias)}
          aria-busy=${this._isSearching ? 'true' : 'false'}
        >
          <!-- Screen reader status announcements -->
          <div class="visually-hidden" role="status" aria-live="polite" aria-atomic="true">
            ${this._searchStatusMessage}
          </div>

          ${when(
            this.#isSearchDisabled,
            () => html`
              <div class="search-disabled-message">
                <umb-localize key="search_searchDisabled">
                  Search is disabled because the index is not healthy. Current status:
                </umb-localize>
                ${this.localize.term('search_healthStatus', this._healthStatus)}
              </div>
            `,
          )}

          <div class="search-input-row">
            <uui-input
              id="search-input"
              .value=${this.#inputValue}
              @input=${this.#handleInputChange}
              @keydown=${this.#handleKeyDown}
              ?disabled=${this.#isSearchDisabled}
              placeholder=${this.localize.term('search_searchPlaceholder')}
              label=${this.localize.term('search_searchInputLabel')}
              aria-label=${this.localize.term('search_searchInputAriaLabel', this._indexAlias)}
              aria-describedby="search-hint"
            >
              <uui-icon
                name="icon-search"
                slot="prepend"
                style="padding-left:var(--uui-size-space-2)"
              ></uui-icon>
            </uui-input>
            ${when(
              this._hasMultipleLanguages,
              () => html`
                <uui-select
                  label=${this.localize.term('search_cultureSelectLabel')}
                  .options=${this._languages.map((lang) => ({
                    name: lang.name,
                    value: lang.unique,
                    selected: lang.unique === this._selectedCulture,
                  }))}
                  @change=${this.#handleCultureChange}
                ></uui-select>
              `,
            )}
            <uui-button
              look="primary"
              color="positive"
              @click=${this.#handleButtonClick}
              ?disabled=${this.#isSearchDisabled || this._isSearching}
              label=${this.localize.term('search_searchButtonAriaLabel')}
            >
              <umb-localize key="search_searchButton">Search</umb-localize>
            </uui-button>
          </div>

          <div id="search-hint" class="visually-hidden">
            <umb-localize key="search_searchHint">
              Press Enter or click Search button to execute search
            </umb-localize>
          </div>
          ${when(
            this._isSearching,
            () => html`
              <div role="status" aria-label=${this.localize.term('search_loading')}>
                <uui-loader></uui-loader>
              </div>
            `,
          )}
          ${when(
            this._error,
            () => html`
              <div class="error-message" role="alert" aria-live="assertive">${this._error}</div>
            `,
          )}
          ${this.#renderResults()}
        </div>
      </uui-box>
    `;
  }

  async #handleSearch() {
    // Prevent concurrent searches
    if (this._isSearching) {
      return;
    }

    // Sync state with current input value
    this._searchQuery = this.#inputValue;

    if (!this._indexAlias) {
      return;
    }

    this._isSearching = true;
    this._error = undefined;
    this._searchStatusMessage = this.localize.term('search_searching');

    const initialPage = this.#initialPage;
    this.#initialPage = undefined;

    const skip = initialPage ? (initialPage - 1) * PAGE_SIZE : this.#pagination.getSkip();

    const request: UmbSearchRequest = {
      indexAlias: this._indexAlias,
      query: this._searchQuery,
      culture: this._selectedCulture,
      skip,
      take: PAGE_SIZE,
    };

    const { data, error } = await this.#queryRepository.search(request);

    if (error || !data) {
      this._error = error?.message ?? this.localize.term('search_searchError');
      this._searchResults = undefined;
      this._tableItems = [];
      this._searchStatusMessage = this.localize.term('search_searchFailed');
    } else {
      this._searchResults = data;
      this.#pagination.setTotalItems(data.total);
      if (initialPage) {
        this.#pagination.setCurrentPageNumber(initialPage);
      }
      this.#createTableItems(data);
      this._searchStatusMessage = this.localize.term('search_searchComplete', data.total);
    }

    this._isSearching = false;
    this.#updateUrlParams();
  }

  #createTableItems(results: UmbSearchResult) {
    this._tableItems = results.documents.map((doc) => ({
      id: `${doc.unique}_${this._selectedCulture}`,
      icon: doc.icon,
      data: [
        {
          columnAlias: 'name',
          value: html`
            <div style="padding: var(--uui-size-2) 0;">
              <uui-button
                look="secondary"
                label="Open"
                aria-label=${this.localize.term('search_openEntity', doc.entityType, doc.unique)}
                href=${this.#getModalUrl(doc.unique, doc.entityType)}
              >
                ${doc.name}
              </uui-button>
              <div><small>${doc.unique}</small></div>
            </div>
          `,
        },
        {
          columnAlias: 'entityType',
          value: doc.entityType,
        },
        {
          columnAlias: 'actions',
          value: html`<umb-entity-actions-table-column-view
            .value=${{
              unique: doc.unique,
              entityType: UMB_SEARCH_DOCUMENT_ENTITY_TYPE,
              name: doc.name,
            }}
          ></umb-entity-actions-table-column-view>`,
        },
      ],
    }));
  }

  #getModalUrl(id: string, entityType: string): string {
    if (!this.#routeBuilder) {
      console.error('Route builder not initialized');
      return '#';
    }

    const modalPath = this.#routeBuilder({ entityType });
    return `${modalPath}edit/${id}`;
  }

  #handleInputChange(e: Event) {
    const input = e.target as UUIInputElement;
    this.#inputValue = input.value as string;
    this.#pagination.setCurrentPageNumber(1);
    this.#debouncedSearch();
  }

  #handleKeyDown(e: KeyboardEvent) {
    if (e.key === 'Enter') {
      // Execute search immediately (debounced search will be skipped if already searching)
      void this.#handleSearch();
    }
  }

  #handleButtonClick() {
    // Execute search immediately (debounced search will be skipped if already searching)
    void this.#handleSearch();
  }

  #handleCultureChange(e: Event) {
    const select = e.target as UUISelectElement;
    this.#pagination.setCurrentPageNumber(1);
    // Setting culture on workspace context triggers the observeMultiple callback which fires #handleSearch
    this.#workspaceContext?.setSelectedCulture(select.value as string);
  }

  #onPageChange(event: Event) {
    const target = event.target as UUIPaginationElement;
    this.#pagination.setCurrentPageNumber(target.current);
    void this.#handleSearch();
  }

  #renderResults() {
    if (!this._searchResults) return nothing;

    if (this._searchResults.total === 0) {
      return html`
        <div class="no-results" role="status" aria-live="polite">
          <umb-localize key="search_noResults">No results found</umb-localize>
        </div>
      `;
    }

    return html`
      <div
        class="results-container"
        role="region"
        aria-label=${this.localize.term('search_resultsRegion')}
      >
        <div class="results-header" id="results-summary">
          <strong>
            <umb-localize key="search_resultsCount" .args=${[this._searchResults.total]}>
              Found ${this._searchResults.total} result${this._searchResults.total === 1 ? '' : 's'}
            </umb-localize>
          </strong>
        </div>
        <umb-table
          .config=${this._tableConfig}
          .columns=${this._tableColumns}
          .items=${this._tableItems}
          aria-describedby="results-summary"
          aria-label=${this.localize.term('search_resultsTable')}
        >
        </umb-table>
        ${
          this._totalPages > 1
            ? html`
                <uui-pagination
                  .current=${this._currentPage}
                  .total=${this._totalPages}
                  ?disabled=${this._isSearching}
                  @change=${this.#onPageChange}
                  aria-label=${this.localize.term('search_paginationLabel')}
                ></uui-pagination>
              `
            : nothing
        }
      </div>
    `;
  }
  static override styles = [
    UmbTextStyles,
    css`
      :host {
        display: block;
      }

      .search-container {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-4);
      }

      /* Visually hidden but accessible to screen readers */
      .visually-hidden {
        position: absolute;
        width: 1px;
        height: 1px;
        padding: 0;
        margin: -1px;
        overflow: hidden;
        clip: rect(0, 0, 0, 0);
        white-space: nowrap;
        border-width: 0;
      }

      .search-input-row {
        display: flex;
        gap: var(--uui-size-space-3);
        align-items: flex-end;
      }

      uui-input {
        flex: 1;
      }

      .error-message {
        padding: var(--uui-size-space-4);
        background-color: var(--uui-color-danger-standalone);
        color: var(--uui-color-danger-contrast);
        border-radius: var(--uui-border-radius);
      }

      .search-disabled-message {
        color: var(--uui-color-danger);
        font-size: 0.875rem;
      }

      .no-results {
        padding: var(--uui-size-space-4);
        text-align: center;
        color: var(--uui-color-text-alt);
      }

      .results-container {
        display: flex;
        flex-direction: column;
        gap: var(--uui-size-space-3);
      }

      .results-header {
        padding-bottom: var(--uui-size-space-2);
        border-bottom: 1px solid var(--uui-color-border);
        margin-bottom: var(--uui-size-space-3);
      }

      uui-pagination {
        display: flex;
        justify-content: center;
      }
    `,
  ];
}

export default UmbSearchIndexSearchBoxElement;

declare global {
  interface HTMLElementTagNameMap {
    'umb-search-index-search-box': UmbSearchIndexSearchBoxElement;
  }
}
