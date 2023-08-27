import { UmbDatasetContext } from "./dataset-context.interface.js";
import { UmbVariantId } from "@umbraco-cms/backoffice/variant";


export interface UmbVariantDatasetContext extends UmbDatasetContext {
	getVariantId: () => UmbVariantId | undefined;
}
