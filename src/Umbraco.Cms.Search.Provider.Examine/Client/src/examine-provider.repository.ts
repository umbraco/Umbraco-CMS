import { getDocument } from './api';
import type { ExamineDocument } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

export class UmbSearchExamineProviderRepository extends UmbRepositoryBase {
  constructor(host: UmbControllerHost) {
    super(host);
  }

  async requestSearchDocument(documentKey: string | undefined, indexAlias: string | undefined) {
    if (!documentKey) {
      return { error: new Error('Search document documentKey identifier is not provided') };
    }

    if (!indexAlias) {
      return { error: new Error('Index alias is not provided') };
    }

    const { data, error } = await tryExecute(
      this,
      getDocument({
        client: umbHttpClient as never,
        path: {
          documentKey,
          indexAlias,
        },
      }),
    );

    return { data: data as ExamineDocument | undefined, error };
  }
}
