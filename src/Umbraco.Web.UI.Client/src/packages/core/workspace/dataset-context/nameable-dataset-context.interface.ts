import { UmbDatasetContext } from "./dataset-context.interface.js";

/**
 * A dataset with ability to set the name of it.
*/
export interface UmbNameableDatasetContext extends UmbDatasetContext {
	setName(name:string): void
}
