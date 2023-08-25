import { type UmbDatasetContext } from "./dataset-context.interface.js";
import { UmbVariantDatasetContext } from "./variant-dataset-context.interface.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const UMB_VARIANT_DATASET_CONTEXT = new UmbContextToken<UmbDatasetContext, UmbVariantDatasetContext>(
	"UmbEntityContext",
(context): context is UmbVariantDatasetContext => 'getVariantId' in context);
