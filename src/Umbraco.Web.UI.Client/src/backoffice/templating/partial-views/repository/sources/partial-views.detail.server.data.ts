import { ProblemDetailsModel, TemplateResponseModel, TemplateResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface PartialViewDetailDataSource {
	//createScaffold(): Promise<DataSourceResponse<TemplateResponseModel>>;
	get(key: string): Promise<DataSourceResponse<TemplateResponseModel>>;
	//insert(template: TemplateResponseModel): Promise<DataSourceResponse>;
	update(template: TemplateResponseModel): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
}

export class UmbPartialViewDetailServerDataSource implements PartialViewDetailDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbPartialViewDetailServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	get(key: string) {
		// return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateByKey({ key }));
		return Promise.reject(new Error('Not implemented'));
	}

	async update(partial: any) {
		// if (!template.key) {
		// 	const error: ProblemDetailsModel = { title: 'Template key is missing' };
		// 	return { error };
		// }

		// const payload = { key: template.key, requestBody: template };
		// return tryExecuteAndNotify(this.#host, TemplateResource.putTemplateByKey(payload));
		return Promise.reject(new Error('Not implemented'));
	}

	async delete(key: string) {
		// if (!key) {
		// 	const error: ProblemDetailsModel = { title: 'Key is missing' };
		// 	return { error };
		// }

		// return await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateByKey({ key }));
		return Promise.reject(new Error('Not implemented'));
	}
}
