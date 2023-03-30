import { PartialViewsTreeDataSource } from '.';
import { PartialViewResource, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbPartialViewsTreeServerDataSource implements PartialViewsTreeDataSource {
	#host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
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
			const error: ProblemDetailsModel = { title: 'Path is missing' };
			return { error };
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

	async getItems(paths: Array<string>) {
		if (!paths) {
			const error: ProblemDetailsModel = { title: 'Paths are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			PartialViewResource.getTreePartialViewItem({
				path: paths,
			})
		);
	}
}
