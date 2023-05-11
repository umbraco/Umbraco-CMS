import { PartialViewDetails } from '../../config';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { DataSourceResponse, UmbDataSource } from '@umbraco-cms/backoffice/repository';

//TODO Pass proper models
export class UmbPartialViewDetailServerDataSource
	implements UmbDataSource<PartialViewDetails, PartialViewDetails, PartialViewDetails, PartialViewDetails>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbPartialViewDetailServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbPartialViewDetailServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
	update(unique: string, data: PartialViewDetails): Promise<DataSourceResponse<PartialViewDetails>> {
		throw new Error('Method not implemented.');
	}
	delete(unique: string): Promise<DataSourceResponse> {
		throw new Error('Method not implemented.');
	}
}
