import { type UmbDatasetContext } from "./dataset-context.interface.js";
import { UmbVariantDatasetContext } from "./variant-dataset-context.interface.js";
import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";

export const IsVariantDatasetContext = (context: UmbDatasetContext): context is UmbVariantDatasetContext => 'getVariantId' in context;

export const UMB_VARIANT_DATASET_CONTEXT = new UmbContextToken<UmbDatasetContext, UmbVariantDatasetContext>(
	"UmbDatasetContext",
	IsVariantDatasetContext);
