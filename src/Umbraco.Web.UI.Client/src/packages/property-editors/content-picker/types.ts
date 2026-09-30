import type { UmbContentPickerDynamicRoot } from '@umbraco-cms/backoffice/content';

export type * from './dynamic-root/types.js';
// The dynamic root types now live in the content package, so the pickers that offer one need not depend on the
// content picker. Re-exported here because the names are public API.
export type {
	UmbContentPickerDynamicRoot,
	UmbContentPickerDynamicRootQueryStep,
} from '@umbraco-cms/backoffice/content';

export type UmbContentPickerSourceType = 'content' | 'member' | 'media';

export type UmbContentPickerSource = {
	type: UmbContentPickerSourceType;
	id?: string;
	dynamicRoot?: UmbContentPickerDynamicRoot;
};
