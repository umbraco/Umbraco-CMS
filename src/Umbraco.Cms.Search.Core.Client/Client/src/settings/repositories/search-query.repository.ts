import type { UmbSearchRequest } from '../types.js';
import { UmbSearchQueryServerDataSource } from './search-query.server.data-source.js';

import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbSearchQueryRepository extends UmbRepositoryBase {
  #dataSource = new UmbSearchQueryServerDataSource(this);

  async search(request: UmbSearchRequest) {
    return this.#dataSource.search(request);
  }
}
