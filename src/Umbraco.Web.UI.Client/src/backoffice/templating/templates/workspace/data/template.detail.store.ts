import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { Template } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

/**
 * @export
 * @class UmbTemplateDetailStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Template Details
 */
export class UmbTemplateDetailStore extends UmbStoreBase {
	#data = new ArrayState<Template>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UmbTemplateDetailStore.name);
	}

	append(template: Template) {
		this.#data.append([template]);
	}
}

export const UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateDetailStore>(
	UmbTemplateDetailStore.name
);
