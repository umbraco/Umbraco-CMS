import type { UmbContentPickerDynamicRoot } from '@umbraco-cms/backoffice/dynamic-root';

export type * from './dynamic-root/types.js';
// The dynamic root types now live in their own module, so the pickers that offer one need not depend on the
// content picker. Re-exported here because the names are public API.
export type * from '@umbraco-cms/backoffice/dynamic-root';

export type UmbContentPickerSourceType = 'content' | 'member' | 'media';

export type UmbContentPickerSource = {
	type: UmbContentPickerSourceType;
	id?: string;
	dynamicRoot?: UmbContentPickerDynamicRoot;
};
