import type { JsLoaderProperty } from '@umbraco-cms/backoffice/extension-api';

export interface UmbIconDefinition<JsType = any> {
	name: string;
	path: JsLoaderProperty<JsType>;
	legacy?: boolean;
}

export type UmbIconDictionary = Array<UmbIconDefinition>;

export interface UmbIconModule {
	default: string;
}
