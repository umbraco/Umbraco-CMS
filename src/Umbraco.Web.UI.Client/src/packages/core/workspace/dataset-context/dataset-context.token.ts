import { type UmbDatasetContext } from "./dataset-context.interface.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const UMB_DATASET_CONTEXT = new UmbContextToken<UmbDatasetContext>("UmbEntityContext");
