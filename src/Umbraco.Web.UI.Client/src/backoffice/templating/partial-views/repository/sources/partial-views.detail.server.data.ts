import { PartialViewDetails } from '../../config';
import type { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';

export class UmbPartialViewDetailServerDataSource implements UmbDataSource<PartialViewDetails> {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbPartialViewDetailServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	createScaffold(parentKey: string | null): Promise<DataSourceResponse<PartialViewDetails>> {
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
		console.log('GET PATRIAL WITH PATH', path);
		return { data: undefined, error: undefined };
	}

	insert(data: any): Promise<DataSourceResponse<PartialViewDetails>> {
		throw new Error('Method not implemented.');
	}
	update(data: PartialViewDetails): Promise<DataSourceResponse<PartialViewDetails>> {
		throw new Error('Method not implemented.');
	}
	delete(key: string): Promise<DataSourceResponse<PartialViewDetails>> {
		throw new Error('Method not implemented.');
	}
}
