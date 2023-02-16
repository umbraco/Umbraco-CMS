import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbEntityActionBase } from '../../../shared/entity-actions';
import { UmbMediaTypeRepository } from '../repository/media-type.repository';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export default class UmbReloadMediaTypeEntityAction extends UmbEntityActionBase<UmbMediaTypeRepository> {
	static styles = [UUITextStyles];

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		alert('refresh')
	}
}