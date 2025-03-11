import type { JsLoaderProperty } from '@umbraco-cms/backoffice/extension-api';

export type * from './extensions/icons.extension.js';

export interface UmbIconDefinition<JsType = any> {
	name: string;
	path: JsLoaderProperty<JsType>;
	/**
	 * @deprecated `legacy` is deprecated and will be removed in v.17. Use `hidden` instead.
	 */
	legacy?: boolean;
	hidden?: boolean;
}

export type UmbIconDictionary = Array<UmbIconDefinition>;

export interface UmbIconModule {
	default: string;
}
