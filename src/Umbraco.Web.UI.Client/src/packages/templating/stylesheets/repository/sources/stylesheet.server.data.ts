import { StylesheetDetails } from '../..';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for the Stylesheet that fetches data from the server
 * @export
 * @class UmbStylesheetServerDataSource
 * @implements {UmbStylesheetServerDataSource}
 */
export class UmbStylesheetServerDataSource implements UmbDataSource<any, any, any, StylesheetDetails> {
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
		console.log('GET STYLESHEET WITH PATH', path);
		return { data: undefined, error: undefined };
	}

	insert(data: StylesheetDetails): Promise<DataSourceResponse<StylesheetDetails>> {
		throw new Error('Method not implemented.');
	}
	update(path: string, data: StylesheetDetails): Promise<DataSourceResponse<StylesheetDetails>> {
		throw new Error('Method not implemented.');
	}
	delete(path: string): Promise<DataSourceResponse> {
		throw new Error('Method not implemented.');
	}
}
