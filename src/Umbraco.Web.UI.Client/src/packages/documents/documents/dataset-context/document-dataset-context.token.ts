import type { UmbDocumentDatasetContext } from "./document-dataset-context.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import { UmbDatasetContext } from "@umbraco-cms/backoffice/workspace";

export const IsDocumentDatasetContext = (context: UmbDatasetContext): context is UmbDocumentDatasetContext => context.getType() === 'document';

export const UMB_DOCUMENT_DATASET_CONTEXT = new UmbContextToken<UmbDatasetContext, UmbDocumentDatasetContext>(
	"UmbDatasetContext",
	IsDocumentDatasetContext);
