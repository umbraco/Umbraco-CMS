import { TemplateTreeDataSource } from '.';
import { ProblemDetails, TemplateResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

export class TemplateTreeServerDataSource implements TemplateTreeDataSource {
	#host: UmbControllerHostInterface;
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	async getRootItems() {
		return tryExecuteAndNotify(this.#host, TemplateResource.getTreeTemplateRoot({}));
	}

	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateChildren({
				parentKey,
			})
		);
	}

	async getItems(keys: Array<string>) {
		if (keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			TemplateResource.getTreeTemplateItem({
				key: keys,
			})
		);
	}
}
