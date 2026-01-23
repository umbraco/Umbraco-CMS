import { UMB_IS_MODAL_CONDITION_ALIAS } from './constants.js';
import { UmbIsModalCondition } from './is-modal.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Is Modal Condition',
	alias: UMB_IS_MODAL_CONDITION_ALIAS,
	api: UmbIsModalCondition,
};
