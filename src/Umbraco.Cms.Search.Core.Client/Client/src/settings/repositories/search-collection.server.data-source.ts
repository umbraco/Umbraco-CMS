import { indexes } from '../api';
import type { UmbSearchCollectionDataSource, UmbSearchIndex } from '../types.js';
import { UMB_SEARCH_INDEX_ENTITY_TYPE } from '@umbraco-cms/search/global';

import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { client } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSearchCollectionServerDataSource implements UmbSearchCollectionDataSource {
  readonly #host;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  async getCollection(
    _filter: unknown,
  ): Promise<UmbDataSourceResponse<UmbPagedModel<UmbSearchIndex>>> {
    const { data, error } = await tryExecute(
      this.#host,
      indexes({
        client: client,
      }),
    );

    if (error || !data) {
      return { error };
    }

    const items = data.items.map<UmbSearchIndex>((item) => {
      // Derive UI state from server health status
      let state: UmbSearchIndex['state'] = 'idle';
      if (item.healthStatus === 'Rebuilding') {
        state = 'loading';
      } else if (item.healthStatus === 'Corrupted') {
        state = 'error';
      }

      return {
        unique: item.indexAlias,
        name: item.indexAlias,
        providerName: item.providerName,
        documentCount: item.documentCount,
        healthStatus: item.healthStatus,
        entityType: UMB_SEARCH_INDEX_ENTITY_TYPE,
        state,
      };
    });

    return { data: { items, total: data.total } };
  }
}
