import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../../../core/stores/store';
import type { DocumentBlueprintDetails, DocumentDetails } from '@umbraco-cms/models';
import { DocumentBlueprintTreeItem } from '@umbraco-cms/backend-api';
import { UmbContextAlias } from '@umbraco-cms/context-api';

export type UmbDocumentStoreItemType = DocumentBlueprintDetails | DocumentBlueprintTreeItem;

export const STORE_ALIAS = 'umbDocumentBlueprintStore';

const isDocumentBlueprintDetails = (
	documentBlueprint: DocumentBlueprintDetails | DocumentBlueprintTreeItem
): documentBlueprint is DocumentBlueprintDetails => {
	return (documentBlueprint as DocumentBlueprintDetails).data !== undefined;
};

/**
 * @export
 * @class UmbDocumentStore
 * @extends {UmbDocumentStoreBase<DocumentDetails | DocumentTreeItem>}
 * @description - Data Store for Documents
 */
export class UmbDocumentBlueprintStore extends UmbNodeStoreBase<UmbDocumentStoreItemType> {
	public readonly storeAlias = STORE_ALIAS;

	getByKey(key: string): Observable<DocumentDetails | null> {
		// TODO: implement call to end point
		return this.items.pipe(
			map(
				(documentBlueprints) =>
					(documentBlueprints.find(
						(documentBlueprint) => documentBlueprint.key === key && isDocumentBlueprintDetails(documentBlueprint)
					) as DocumentDetails) || null
			)
		);
	}

	// TODO: implement call to end point
	save(): any {
		return;
	}
}

export const UMB_DOCUMENT_BLULEPRINT_STORE_CONTEXT_ALIAS = new UmbContextAlias<UmbDocumentBlueprintStore>(STORE_ALIAS);
