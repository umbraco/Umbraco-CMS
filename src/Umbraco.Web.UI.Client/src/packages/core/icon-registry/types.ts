import type { JsLoaderProperty } from '@umbraco-cms/backoffice/extension-api';

export type * from './extensions/icons.extension.js';

export interface UmbIconDefinition<JsType = any> {
	name: string;
	path: JsLoaderProperty<JsType>;
	hidden?: boolean;
	keywords?: Array<string>;
	groups?: Array<string>;
	related?: Array<string>;
}

export type UmbIconDictionary = Array<UmbIconDefinition>;

export interface UmbIconModule {
	default: string;
}
