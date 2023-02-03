import { UmbDocumentRepository } from '../repository/document.repository';
import { UmbEntityActionBase } from '../../../shared/components/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	constructor(host: UmbControllerHostInterface, unique: string) {
		super(host, UmbDocumentRepository, unique);
	}

	execute() {
		alert('open create dialog');
	}
}
