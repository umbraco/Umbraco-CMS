import { PartialViewDetails } from '../../config.js';
import {
	CreatePartialViewRequestModel,
	PagedSnippetItemResponseModel,
	PartialViewResource,
	PartialViewResponseModel,
	UpdatePartialViewRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbPartialViewDetailServerDataSource
	implements
		UmbDataSource<
			CreatePartialViewRequestModel,
			string,
			UpdatePartialViewRequestModel,
			PartialViewResponseModel,
			string
		>
{
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Creates a new partial view scaffold
	 *
	 * @param {(string | null)} [parentId=null] You can leave this empty
	 * @param {string} preset Name of the snippet to use as a preset
	 * @return {*}  {Promise<DataSourceResponse<PartialViewDetails>>}
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	createScaffold(parentId: string | null = null, preset: string): Promise<DataSourceResponse<PartialViewDetails>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.getPartialViewSnippetByName({ name: preset }));
	}
	/**
	 * Get possible snippets for partial views
	 *
	 * @param {*} { skip = 0, take = 100 }
	 * @return {*}  {Promise<DataSourceResponse<PagedSnippetItemResponseModel>>}
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	getSnippets({ skip = 0, take = 100 }): Promise<DataSourceResponse<PagedSnippetItemResponseModel>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.getPartialViewSnippet({ skip, take }));
	}

	/**
	 * Fetches a partial view with the given path from the server
	 * @param {string} path
	 * @return {*}
	 * @memberof UmbStylesheetServerDataSource
	 */
	get(path: string) {
		if (!path) throw new Error('Path is missing');
		return tryExecuteAndNotify(this.#host, PartialViewResource.getPartialView({ path }));
	}
	/**
	 * Creates a new partial view
	 *
	 * @param {CreatePartialViewRequestModel} requestBody
	 * @return {*}  {Promise<DataSourceResponse<string>>}
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	insert(requestBody: CreatePartialViewRequestModel): Promise<DataSourceResponse<string>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.postPartialViewFolder({ requestBody }));
	}

	//TODO the parameters here are bit ugly, since unique is already in the request body parameter, but it has to be done to marry the UmbDataSource interface an backend API together... maybe come up with some nicer solution
	/**
	 * Updates a partial view
	 *
	 * @param {string} [unique='']
	 * @param {UpdatePartialViewRequestModel} requestBody
	 * @return {*}  {Promise<DataSourceResponse<any>>}
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	update(unique = '', requestBody: UpdatePartialViewRequestModel): Promise<DataSourceResponse<any>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.putPartialView({ requestBody }));
	}
	/**
	 * Deletes a partial view
	 *
	 * @param {string} path
	 * @return {*}  {Promise<DataSourceResponse>}
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	delete(path: string): Promise<DataSourceResponse> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.deletePartialView({ path }));
	}
}
