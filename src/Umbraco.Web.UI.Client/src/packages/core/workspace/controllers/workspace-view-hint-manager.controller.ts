import type { UmbVariantId } from '../../variant/variant-id.class.js';
import { UmbHintManager, type UmbHint } from '@umbraco-cms/backoffice/utils';

export interface UmbWorkspaceHint extends UmbHint {
	variantId?: UmbVariantId;
}

/**
 * @class UmbWorkspaceViewHintManager
 * @description - Class managing the hints of views in a workspace.
 */
export class UmbWorkspaceViewHintManager extends UmbHintManager<UmbWorkspaceHint> {}
