import { v4 as uuid } from 'uuid';
import { UmbTemplateDetailStore, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN } from './template.detail.store';
import { Template, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';

/* we need to new up the repository from within the element context. We want the correct context for
the notifications to be displayed in the correct place. */

// element -> context -> repository -> (store) -> data source

// TODO: implement data sources
export class UmbTemplateRepository {
	#host: UmbControllerHostInterface;
	#detailStore?: UmbTemplateDetailStore;
	#notificationService?: UmbNotificationService;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		new UmbContextConsumerController(this.#host, UMB_TEMPLATE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
			this.#detailStore = instance;
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
		});
	}

	async new(parentKey: string | null): Promise<{ data?: Template; error?: unknown }> {
		let masterTemplateAlias: string | undefined = undefined;
		let error = undefined;
		let data = undefined;

		// TODO: can we do something so we don't have to call two endpoints?
		if (parentKey) {
			const { data: parentData, error: parentError } = await tryExecuteAndNotify(
				this.#host,
				TemplateResource.getTemplateByKey({ key: parentKey })
			);
			masterTemplateAlias = parentData?.alias;
			error = parentError;
		}

		const { data: scaffoldData, error: scaffoldError } = await tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTemplateScaffold({ masterTemplateAlias })
		);
		error = scaffoldError;

		if (scaffoldData?.content) {
			data = {
				key: uuid(),
				name: '',
				alias: '',
				content: scaffoldData?.content,
			};
		}

		return { data, error };
	}

	async get(key: string): Promise<{ data?: Template; error?: unknown }> {
		const { data, error } = await tryExecuteAndNotify(this.#host, TemplateResource.getTemplateByKey({ key }));
		return { data, error };
	}

	async insert(template: Template): Promise<{ error?: unknown }> {
		const payload = { requestBody: template };
		const { error } = await tryExecuteAndNotify(this.#host, TemplateResource.postTemplate(payload));

		if (!error) {
			const notification = { data: { message: `Template created` } };
			this.#notificationService?.peek('positive', notification);
		}

		this.#detailStore?.append(template);

		return { error };
	}

	async update(template: Template): Promise<{ error?: unknown }> {
		if (!template.key) {
			return { error: 'Template key is missing' };
		}

		const payload = { key: template.key, requestBody: template };
		const { error } = await tryExecuteAndNotify(this.#host, TemplateResource.putTemplateByKey(payload));

		if (!error) {
			const notification = { data: { message: `Template saved` } };
			this.#notificationService?.peek('positive', notification);
		}

		this.#detailStore?.append(template);

		return { error };
	}

	// TODO: delete
	// TODO: tree
}
