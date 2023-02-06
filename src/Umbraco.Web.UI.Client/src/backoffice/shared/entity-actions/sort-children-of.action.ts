import { UmbEntityActionBase } from '../components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSortChildrenOfEntityAction<
	T extends { sortChildrenOf(): Promise<void> }
> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		await this.repository?.sortChildrenOf();
	}
}
