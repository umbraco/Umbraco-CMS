import { UmbMediaTypeRepository } from '../repository/media-type.repository';
import { UmbEntityActionBase } from '../../../../../libs/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbCreateMediaTypeEntityAction extends UmbEntityActionBase<UmbMediaTypeRepository> {
	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		alert('open create dialog');
	}
}
