import { UmbMediaTypeRepository } from '../repository/media-type.repository.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export default class UmbReloadMediaTypeEntityAction extends UmbEntityActionBase<UmbMediaTypeRepository> {
	static styles = [UmbTextStyles];

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
	}

	async execute() {
		alert('refresh');
	}
}
