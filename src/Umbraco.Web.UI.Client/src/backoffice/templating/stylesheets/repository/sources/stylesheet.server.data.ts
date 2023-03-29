import { StylesheetDetails } from '../..';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

/**
 * A data source for the Stylesheet that fetches data from the server
 * @export
 * @class UmbStylesheetServerDataSource
 * @implements {UmbStylesheetServerDataSource}
 */
export class UmbStylesheetServerDataSource implements UmbDataSource<StylesheetDetails> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbStylesheetServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}
	createScaffold(parentKey: string | null): Promise<DataSourceResponse<StylesheetDetails>> {
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
	update(data: StylesheetDetails): Promise<DataSourceResponse<StylesheetDetails>> {
		throw new Error('Method not implemented.');
	}
	delete(key: string): Promise<DataSourceResponse<StylesheetDetails>> {
		throw new Error('Method not implemented.');
	}
}
