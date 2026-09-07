import { search } from '../api';
import type { UmbSearchRequest, UmbSearchResult, UmbSearchDocument } from '../types.js';
import type { SearchRequestModel } from '../api';

import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { client } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbSearchQueryServerDataSource extends UmbControllerBase {
  async search(request: UmbSearchRequest): Promise<UmbDataSourceResponse<UmbSearchResult>> {
    // Map domain types to API types
    const apiRequest: SearchRequestModel = {
      indexAlias: request.indexAlias,
      query: request.query ?? null,
      culture: request.culture ?? null,
    };

    const { data, error } = await tryExecute(
      this,
      search({
        body: apiRequest,
        query: {
          skip: request.skip ?? 0,
          take: request.take ?? 10,
        },
        client: client,
      }),
    );

    if (error || !data) {
      return { error };
    }

    // Map API documents to domain types with defaults
    const documents: UmbSearchDocument[] = data.documents.map((apiDoc) => ({
      unique: apiDoc.id,
      objectType: String(apiDoc.objectType),
      entityType: this.#getEntityType(apiDoc.objectType),
      name: apiDoc.name ?? 'Unknown',
      icon: apiDoc.icon ?? 'icon-document',
    }));

    const result: UmbSearchResult = {
      total: data.total,
      documents,
    };

    return { data: result };
  }

  #getEntityType(objectType: string): string {
    // Map UmbracoObjectTypes enum values to entity type strings
    const typeMap: Record<string, string> = {
      Document: 'document',
      Media: 'media',
      Member: 'member',
      DocumentType: 'document-type',
      MediaType: 'media-type',
      MemberType: 'member-type',
      DataType: 'data-type',
    };

    return typeMap[objectType] || objectType.toLowerCase();
  }
}
