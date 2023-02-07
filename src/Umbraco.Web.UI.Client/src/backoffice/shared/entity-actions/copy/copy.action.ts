import { UmbActionBase } from '..';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbCopyEntityAction<T extends { copy(): Promise<void> }> extends UmbActionBase<T> {
	#unique: string;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias);
		this.#unique = unique;
	}

	async execute() {
		console.log(`execute for: ${this.#unique}`);
		await this.repository?.copy();
	}
}
