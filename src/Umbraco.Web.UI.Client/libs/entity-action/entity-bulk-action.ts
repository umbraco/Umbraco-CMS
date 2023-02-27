import { UmbAction, UmbActionBase } from './action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export interface UmbEntityBulkAction<RepositoryType> extends UmbAction<RepositoryType> {
	selection: Array<string>;
}

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
