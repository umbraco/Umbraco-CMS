import type { JsLoaderProperty } from '@umbraco-cms/backoffice/extension-api';

export interface UmbIconDefinition<JsType extends object = object> {
	name: string;
	path: JsLoaderProperty<JsType>;
	legacy?: boolean;
}

export type UmbIconDictionary = Array<UmbIconDefinition>;
