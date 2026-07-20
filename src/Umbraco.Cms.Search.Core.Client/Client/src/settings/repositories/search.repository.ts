import { rebuild } from '../api';

import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { client } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbSearchRepository extends UmbRepositoryBase {
  async rebuildIndex(indexAlias: string): Promise<void> {
    const { error } = await tryExecute(this, rebuild({ query: { indexAlias }, client: client }));
    if (error) throw error;
  }
}
