import { UmbEntityActionBase } from '../../../../../libs/entity-action/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbMoveEntityAction<T extends { move(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.move();
	}
}
