import { type UmbDatasetContext } from "./dataset-context.interface.js";
import { UmbNameableDatasetContext } from "./nameable-dataset-context.interface.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const IsNameableDatasetContext = (context: UmbDatasetContext): context is UmbNameableDatasetContext => 'setName' in context;

export const UMB_NAMEABLE_DATASET_CONTEXT = new UmbContextToken<UmbDatasetContext, UmbNameableDatasetContext>(
	"UmbDatasetContext",
	IsNameableDatasetContext);
