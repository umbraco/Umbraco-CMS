import type {
	UmbDocumentBlueprintEntityType,
	UmbDocumentBlueprintFolderEntityType,
	UmbDocumentBlueprintRootEntityType,
} from '../entity.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentBlueprintTreeRootModel extends UmbTreeRootModel {
	entityType: UmbDocumentBlueprintRootEntityType;
}

export interface UmbDocumentBlueprintTreeItemModel extends UmbTreeItemModel {
	entityType: UmbDocumentBlueprintEntityType | UmbDocumentBlueprintFolderEntityType;
	variants: Array<UmbDocumentBlueprintTreeItemVariantModel>;
}

export interface UmbDocumentBlueprintTreeItemVariantModel {
	name: string;
	culture: string | null;
	segment: string | null;
}
