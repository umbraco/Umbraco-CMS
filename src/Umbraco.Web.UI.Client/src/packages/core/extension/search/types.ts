import type { UmbSearchRequestArgs as UmbBaseSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export interface UmbExtensionSearchRequestArgs extends UmbBaseSearchRequestArgs {
	extensionTypes?: Array<string>;
}
