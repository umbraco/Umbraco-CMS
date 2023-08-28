import type { UmbDataTypeDatasetContext } from "./data-type-dataset-context.js";
import { UmbDatasetContext } from "@umbraco-cms/backoffice/workspace";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const UMB_DATA_TYPE_DATASET_CONTEXT = new UmbContextToken<UmbDatasetContext, UmbDataTypeDatasetContext>(
	"UmbDatasetContext",
(context): context is UmbDataTypeDatasetContext => 'properties' in context && context.getType() === 'data-type');
