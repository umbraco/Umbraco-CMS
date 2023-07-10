import { PartialViewsTreeDataSource } from './index.js';
import { PartialViewResource, ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbPartialViewsTreeServerDataSource implements PartialViewsTreeDataSource {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	async getRootItems() {
		return tryExecuteAndNotify(this.#host, PartialViewResource.getTreePartialViewRoot({}));
	}

	async getChildrenOf({
		path,
		skip,
		take,
	}: {
		path?: string | undefined;
		skip?: number | undefined;
		take?: number | undefined;
	}) {
		if (!path) {
			const error: ProblemDetails = { title: 'Path is missing' };
			return error;
		}

		return tryExecuteAndNotify(
			this.#host,
			PartialViewResource.getTreePartialViewChildren({
				path,
				skip,
				take,
			})
		);
	}

	async getItem(id: Array<string>) {
		if (!id) {
			const error: ProblemDetails = { title: 'Paths are missing' };
			return error;
		}

		return tryExecuteAndNotify(
			this.#host,
			PartialViewResource.getPartialViewItem({
				id,
			})
		);
	}
}
