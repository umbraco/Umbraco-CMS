import { type UmbVariantId } from "../../variant/variant-id.class.js";
import { UmbDatasetContext } from "./dataset-context.interface.js";
import { type Observable } from "@umbraco-cms/backoffice/external/rxjs";


export interface UmbVariantDatasetContext extends UmbDatasetContext {

	getVariantId(): UmbVariantId;

}
