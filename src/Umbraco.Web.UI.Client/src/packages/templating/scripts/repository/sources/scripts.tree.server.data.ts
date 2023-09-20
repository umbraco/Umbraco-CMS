import { ScriptsTreeDataSource } from './index.js';
import { ScriptResource, ProblemDetails } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbScriptsTreeServerDataSource implements ScriptsTreeDataSource {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	async getRootItems() {
		return tryExecuteAndNotify(this.#host, ScriptResource.getTreeScriptRoot({}));
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
			ScriptResource.getTreeScriptChildren({
				path,
				skip,
				take,
			}),
		);
	}

	async getItem(path: Array<string>) {
		if (!path) {
			const error: ProblemDetails = { title: 'Paths are missing' };
			return error;
		}

		return tryExecuteAndNotify(
			this.#host,
			ScriptResource.getScriptItem({
				path,
			}),
		);
	}
}
