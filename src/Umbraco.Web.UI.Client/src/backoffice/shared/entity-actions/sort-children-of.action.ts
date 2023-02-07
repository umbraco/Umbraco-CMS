import { UmbActionBase } from '../components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSortChildrenOfEntityAction<T extends { sortChildrenOf(): Promise<void> }> extends UmbActionBase<T> {
	#unique: string;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias);
		this.#unique = unique;
	}

	async execute() {
		console.log(`execute for: ${this.#unique}`);
		await this.repository?.sortChildrenOf();
	}
}
