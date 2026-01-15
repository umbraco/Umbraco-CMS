import type { JsLoaderProperty } from '@umbraco-cms/backoffice/extension-api';

export type * from './extensions/icons.extension.js';

export interface UmbIconDefinition<JsType = any> {
	name: string;
	path: JsLoaderProperty<JsType>;
	hidden?: boolean;
}

export type UmbIconDictionary = Array<UmbIconDefinition>;

export interface UmbIconModule {
	default: string;
}
