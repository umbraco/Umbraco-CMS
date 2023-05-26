import { UmbEntityActionBase } from '../../entity-action.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

// TODO: investigate what we need to finish the generic move action. We would need to open a picker, which requires a modal token,
// maybe we can use kinds to make a specific manifest to the move action.
export class UmbMoveEntityAction<T extends { move(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.move();
	}
}
