import { UmbEntityActionBase } from '../components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbMoveEntityAction<T extends { move(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		await this.repository?.move();
	}
}
