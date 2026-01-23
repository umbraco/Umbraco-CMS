import { UMB_IN_MODAL_CONDITION_ALIAS } from './constants.js';
import { UmbInModalCondition } from './in-modal.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'In Modal Condition',
	alias: UMB_IN_MODAL_CONDITION_ALIAS,
	api: UmbInModalCondition,
};
