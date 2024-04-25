import type { UmbDynamicRoot, UmbDynamicRootQueryStep } from '@umbraco-cms/backoffice/dynamic-root';

export type UmbContentPickerSourceType = 'content' | 'member' | 'media';

export type UmbContentPickerSource = {
	type: UmbContentPickerSourceType;
	id?: string;
	dynamicRoot?: UmbContentPickerDynamicRoot;
};

export interface UmbContentPickerDynamicRoot extends UmbDynamicRoot<UmbContentPickerDynamicRootQueryStep> {}

export interface UmbContentPickerDynamicRootQueryStep extends UmbDynamicRootQueryStep {
	anyOfDocTypeKeys?: Array<string>;
}
