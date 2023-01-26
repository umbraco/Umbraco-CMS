import { UmbContextToken } from '@umbraco-cms/context-api';
import { createObservablePart, ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { Template, TemplateCreateModel, TemplateResource, TemplateUpdateModel } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
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

	getByKey(key: string) {
		tryExecuteAndNotify(this._host, TemplateResource.getTemplateByKey({ key })).then(({ data }) => {
			if (data) {
				// TODO: how do we handle if an item has been removed during this session(like in another tab or by another user)?
				this.#data.append([data]);
			}
		});

		return createObservablePart(this.#data, (items) => items.find((item) => item.key === key));
	}

	async save(template: Template) {
		if (!template.key) return;

		const { error } = await tryExecuteAndNotify(
			this._host,
			TemplateResource.putTemplateByKey({ key: template.key, requestBody: template })
		);

		if (error) throw error;

		this.#data.append([template]);
	}

	async create(template: TemplateCreateModel) {
		const { data } = await tryExecuteAndNotify(this._host, TemplateResource.postTemplate({ requestBody: template }));
		if (data) {
			this.#data.append([data]);
		}
	}
}

export const UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbTemplateDetailStore>(
	UmbTemplateDetailStore.name
);
