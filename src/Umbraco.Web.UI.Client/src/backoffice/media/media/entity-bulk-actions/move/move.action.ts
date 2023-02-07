import type { UmbMediaRepository } from '../../repository/media.repository';
import { UmbActionBase } from '../../../../shared/entity-actions';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbMediaMoveEntityBulkAction extends UmbActionBase<UmbMediaRepository> {
	#selection: Array<string>;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.#selection = selection;
	}

	setSelection(selection: Array<string>) {
		this.#selection = selection;
	}

	async execute() {
		console.log(`execute move for: ${this.#selection}`);
		await this.repository?.move();
	}
}
