import { UMB_CURRENT_USER_DOCUMENT_BLUEPRINT_ACCESS_CONDITION_ALIAS } from './constants.js';
import { UmbCurrentUserDocumentBlueprintAccessCondition } from './document-blueprint-access.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Current user has document blueprint access Condition',
	alias: UMB_CURRENT_USER_DOCUMENT_BLUEPRINT_ACCESS_CONDITION_ALIAS,
	api: UmbCurrentUserDocumentBlueprintAccessCondition,
};
