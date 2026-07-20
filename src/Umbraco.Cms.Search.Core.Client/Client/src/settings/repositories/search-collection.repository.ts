import type { UmbSearchIndex } from '../types.js';
import { UmbSearchCollectionServerDataSource } from './search-collection.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class UmbSearchCollectionRepository
  extends UmbRepositoryBase
  implements UmbCollectionRepository<UmbSearchIndex, never>
{
  #collectionSource = new UmbSearchCollectionServerDataSource(this);

  async requestCollection(filter: unknown) {
    return this.#collectionSource.getCollection(filter);
  }
}
