import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbEntityActionBase } from '@umbraco-cms/entity-action';
import { UmbDictionaryRepository } from '../repository/dictionary.repository';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export default class UmbReloadDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		alert('refresh');
	}
}
