import { v4 as uuid } from 'uuid';
import { TemplateDetailDataSource } from '.';
import { ProblemDetails, Template, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbTemplateDetailServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbTemplateDetailServerDataSource implements TemplateDetailDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbTemplateDetailServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches a Template with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	get(key: string) {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTemplateByKey({ key }));
	}

	/**
	 * Creates a new Template scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
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

	/**
	 * Inserts a new Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async insert(template: Template) {
		const payload = { requestBody: template };
		return tryExecuteAndNotify(this.#host, TemplateResource.postTemplate(payload));
	}

	/**
	 * Updates a Template on the server
	 * @param {Template} template
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async update(template: Template) {
		if (!template.key) {
			const error: ProblemDetails = { title: 'Template key is missing' };
			return { error };
		}

		const payload = { key: template.key, requestBody: template };
		return tryExecuteAndNotify(this.#host, TemplateResource.putTemplateByKey(payload));
	}

	/**
	 * Deletes a Template on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbTemplateDetailServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetails = { title: 'Key is missing' };
			return { error };
		}

		return await tryExecuteAndNotify(this.#host, TemplateResource.deleteTemplateByKey({ key }));
	}
}
