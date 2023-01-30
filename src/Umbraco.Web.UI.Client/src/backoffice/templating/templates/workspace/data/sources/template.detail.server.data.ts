import { v4 as uuid } from 'uuid';
import { TemplateDetailDataSource } from '.';
import { ProblemDetails, Template, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

export class UmbTemplateDetailServerDataSource implements TemplateDetailDataSource {
	#host: UmbControllerHostInterface;
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	get(key: string) {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateByKey({ key }));
	}

	async createScaffold(parentKey: string | null) {
		let masterTemplateAlias: string | undefined = undefined;
		let error = undefined;
		const data: Template = {
			key: uuid(),
			name: '',
			alias: '',
			content: '',
		};

		// TODO: update when backend is updated so we don't have to do two calls
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
		data.content = scaffoldData?.content || '';

		return { data, error };
	}

	async insert(template: Template) {
		const payload = { requestBody: template };
		return tryExecuteAndNotify(this.#host, TemplateResource.postTemplate(payload));
	}

	async update(template: Template) {
		if (!template.key) {
			const error: ProblemDetails = { title: 'Template key is missing' };
			return { error };
		}

		const payload = { key: template.key, requestBody: template };
		return tryExecuteAndNotify(this.#host, TemplateResource.putTemplateByKey(payload));
	}

	async delete(key: string) {
		if (!key) {
			const error: ProblemDetails = { title: 'Key is missing' };
			return { error };
		}

		return await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateByKey({ key }));
	}
}
