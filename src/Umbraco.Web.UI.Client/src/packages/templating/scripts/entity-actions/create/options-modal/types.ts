import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * @deprecated Use the `Umb.EntityAction.Script.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export interface UmbScriptCreateOptionsModalData {
	parent: UmbEntityModel;
}
