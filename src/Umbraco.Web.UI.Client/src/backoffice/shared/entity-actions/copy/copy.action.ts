import { UmbEntityActionBase } from '..';
import { UmbExecutedEvent } from '../../../../core/events';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbCopyEntityAction<T extends { copy(): Promise<void> }> extends UmbEntityActionBase<T> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.copy();
		this.host.dispatchEvent(new UmbExecutedEvent());
	}
}
