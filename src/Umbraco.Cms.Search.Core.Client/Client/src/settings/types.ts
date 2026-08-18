import { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type * from './conditions/types.js';

export type UmbSearchCollectionDataSource = UmbCollectionDataSource<UmbSearchIndex, never>;

export type UmbSearchIndexState = 'idle' | 'loading' | 'error';
export type UmbHealthStatusModel = 'Healthy' | 'Rebuilding' | 'Corrupted' | 'Empty' | 'Unknown';

export type UmbSearchIndex = {
  providerName: string;
  unique: string;
  name: string;
  documentCount: number;
  healthStatus: UmbHealthStatusModel;
  entityType: string;
  state: UmbSearchIndexState;
};

export type UmbSearchRequest = {
  indexAlias: string;
  query?: string;
  culture?: string;
  segment?: string;
  skip?: number;
  take?: number;
};

// Search result types
export type UmbSearchDocument = {
  unique: string;
  objectType: string;
  entityType: string; // Mapped from objectType for easier handling in the UI
  name?: string;
  icon?: string;
};

export type UmbSearchResult = {
  total: number;
  documents: UmbSearchDocument[];
};
