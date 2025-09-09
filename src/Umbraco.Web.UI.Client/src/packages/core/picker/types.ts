export interface UmbPickerContextConfig {
	search?: {
		providerAlias: string;
		queryParams?: object;
	};
}

export type * from './search/types.js';
export type * from './memory/types.js';
