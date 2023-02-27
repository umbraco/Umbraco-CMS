import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbActionBase } from 'src/backoffice/shared/action';

export class UmbEntityBulkActionBase<T> extends UmbActionBase<T> {
	selection: Array<string>;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.selection = selection;
	}

	setSelection(selection: Array<string>) {
		this.selection = selection;
	}
}
