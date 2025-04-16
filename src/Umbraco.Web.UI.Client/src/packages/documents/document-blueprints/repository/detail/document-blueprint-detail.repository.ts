import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDocumentBlueprintDetailModel } from '../../types.js';
import { UmbDocumentBlueprintServerDataSource } from './document-blueprint-detail.server.data-source.js';
import { UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT } from './document-blueprint-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { DocumentBlueprintService, type DocumentBlueprintResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../../entity.js';

export class UmbDocumentBlueprintDetailRepository extends UmbDetailRepositoryBase<UmbDocumentBlueprintDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintServerDataSource, UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_CONTEXT);
	}

	async scaffoldByUnique(unique: string): Promise<UmbDocumentBlueprintDetailModel | undefined> {
		if (!unique) throw new Error('Unique is missing');

		const { data } = await tryExecuteAndNotify(
			this._host,
			DocumentBlueprintService.getDocumentBlueprintByIdScaffold({ id: unique }),
		);

		if (!data) {
			return undefined;
		}

		const blueprintDataSource = this.detailDataSource as UmbDocumentBlueprintServerDataSource;
		return blueprintDataSource.createDocumentBlueprintDetailModel(data);
	}
}

export { UmbDocumentBlueprintDetailRepository as api };
