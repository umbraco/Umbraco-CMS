import type { UmbHealthStatusModel } from '@umbraco-cms/search/settings';
import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
  search: {
    treeHeader: 'Search',
    tableColumnAlias: 'Alias',
    tableColumnHealthStatus: 'Health Status',
    tableColumnDocumentCount: 'Document Count',
    healthStatus: (status: UmbHealthStatusModel) => status,
    documentCount: (cnt: number) => {
      switch (cnt) {
        case 0:
          return 'Empty';
        case 1:
          return '1 document';
        default:
          return `${cnt} documents`;
      }
    },
    collectionActionReload: 'Refresh list',
    entityActionRebuildIndex: 'Rebuild Index',
    rebuildConfirmHeadline: 'Rebuild Search Index',
    rebuildConfirmMessage:
      'Are you sure you want to rebuild the search index? This operation may take a while depending on the size of your content.',
    rebuildConfirmLabel: 'Rebuild Index',
    rebuildStartedMessage:
      'The rebuild of search index "{0}" has started. You can continue working while the process runs in the background.',
    rebuildCompletedTitle: 'Search Index Rebuild Completed',
    rebuildCompletedMessage: 'The rebuild of search index "{0}" has completed successfully.',
    rebuildingIndex: 'Rebuilding index...',
    rebuildIndex: 'Rebuild Index',
    indexInfo: 'Index Information',
    indexAlias: 'Index Alias',
    providerName: 'Provider Name',
    searchBox: 'Search',
    searchPlaceholder: 'Search index...',
    searchButton: 'Search',
    noResults: 'No results found',
    resultsCount: (count: number) => `Found ${count} result${count !== 1 ? 's' : ''}`,
    tableColumnName: 'Name',
    tableColumnEntityType: 'Entity Type',
    statsBoxLabel: 'Statistics',
    searchBoxLabel: 'Search',
    // Accessibility labels
    searching: 'Searching...',
    searchFailed: 'Search failed',
    searchComplete: (count: number) =>
      `Search complete. Found ${count} result${count !== 1 ? 's' : ''}`,
    openEntity: (type: string, id: string) => `Open ${type} with ID ${id}`,
    searchFormLabel: (indexAlias: string) => `Search ${indexAlias} index`,
    searchInputLabel: 'Search query',
    searchInputAriaLabel: (indexAlias: string) => `Enter search query for ${indexAlias} index`,
    searchButtonAriaLabel: 'Execute search',
    searchHint: 'Press Enter or click Search button to execute search',
    loading: 'Loading search results',
    resultsRegion: 'Search results',
    resultsTable: 'Search results table',
    paginationLabel: 'Search results pages',
    cultureSelectLabel: 'Culture',
    searchDisabled: 'Search is disabled because the index is not healthy. Current status:',
    searchError: 'An error occurred while searching. Please try again.',
  },
} satisfies UmbLocalizationDictionary;
