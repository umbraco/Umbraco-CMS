import type { StylesheetDetails } from '../../index.js';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	CreateStylesheetRequestModel,
	StylesheetResource,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Stylesheet that fetches data from the server
 * @export
 * @class UmbStylesheetServerDataSource
 * @implements {UmbStylesheetServerDataSource}
 */
export class UmbStylesheetServerDataSource
	implements UmbDataSource<CreateStylesheetRequestModel, string, UpdateStylesheetRequestModel, StylesheetDetails>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbStylesheetServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}
	createScaffold(parentId: string | null): Promise<DataSourceResponse<StylesheetDetails>> {
		throw new Error('Method not implemented.');
	}

	/**
	 * Fetches a Stylesheet with the given path from the server
	 * @param {string} path
	 * @return {*}
	 * @memberof UmbStylesheetServerDataSource
	 */
	async get(path: string) {
		if (!path) throw new Error('Path is missing');
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheet({ path }));
	}

	insert(data: StylesheetDetails): Promise<DataSourceResponse<any>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.postStylesheet({ requestBody: data }));
	}
	update(path: string, data: StylesheetDetails): Promise<DataSourceResponse<StylesheetDetails>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.putStylesheet({ requestBody: data }));
	}
	delete(path: string): Promise<DataSourceResponse> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.deleteStylesheet({ path }));
	}
}
