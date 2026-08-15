import { index, type IndexModel } from '../api';
import { UMB_SEARCH_INDEX_ENTITY_TYPE } from '@umbraco-cms/search/global';
import type { UmbSearchIndex } from '@umbraco-cms/search/settings';

import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import { client } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute, UmbError } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the search indexes that fetches data from the server
 */
export class UmbSearchServerDataSource implements UmbDetailDataSource<UmbSearchIndex> {
  readonly #host: UmbControllerHost;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  // eslint-disable-next-line @typescript-eslint/require-await
  async createScaffold(preset: Partial<IndexModel> = {}) {
    // Derive UI state from server health status
    let state: UmbSearchIndex['state'] = 'idle';
    if (preset.healthStatus === 'Rebuilding') {
      state = 'loading';
    } else if (preset.healthStatus === 'Corrupted') {
      state = 'error';
    }

    const data: UmbSearchIndex = {
      entityType: UMB_SEARCH_INDEX_ENTITY_TYPE,
      name: preset.indexAlias!,
      providerName: preset.providerName!,
      unique: preset.indexAlias!,
      documentCount: 0,
      state,
      healthStatus: 'Unknown',
      ...preset,
    };

    return { data };
  }

  async read(unique: string) {
    if (!unique) throw new Error('Unique is missing');

    const { data, error } = await tryExecute(
      this.#host,
      index({
        client: client,
        path: {
          indexAlias: unique,
        },
      }),
    );

    if (error || !data) {
      return { error };
    }

    return this.createScaffold(data);
  }

  // eslint-disable-next-line @typescript-eslint/require-await
  async create(model: UmbSearchIndex) {
    console.error('Creating search indexes is not supported.');
    return { data: model, error: new UmbError('Creating search indexes is not supported') };
  }

  // eslint-disable-next-line @typescript-eslint/require-await
  async update(model: UmbSearchIndex) {
    console.error('Updating search indexes is not supported.');
    return { data: model, error: new UmbError('Updating search indexes is not supported') };
  }

  // eslint-disable-next-line @typescript-eslint/require-await
  async delete(_unique: string) {
    console.error('Deleting search indexes is not supported.');
    return { error: new UmbError('Deleting search indexes is not supported') };
  }
}
