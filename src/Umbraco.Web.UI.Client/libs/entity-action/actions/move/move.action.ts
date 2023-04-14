import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

export class UmbMoveEntityAction<T extends { move(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.move();
	}
}
