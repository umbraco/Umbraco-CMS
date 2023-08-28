import { UmbDatasetContext } from "./dataset-context.interface.js";
import { UmbVariantId } from "@umbraco-cms/backoffice/variant";

/**
 * A dataset context for a variant.
 *
 * @notes
 * This one will depending on the origin of such deliver:
 * - name
 * - a variant id
 * - a workspace reference
 * - save state (this should depend on the workspace, this will become another superset of the dataset)
 * - publish state (this should depend on the workspace, this will become another superset of the dataset)
*/
export interface UmbVariantDatasetContext extends UmbDatasetContext {
	getVariantId: () => UmbVariantId;
}
