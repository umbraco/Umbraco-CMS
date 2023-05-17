import { UmbAction, UmbActionBase } from '@umbraco-cms/backoffice/action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityBulkAction<RepositoryType = unknown> extends UmbAction<RepositoryType> {
	selection: Array<string>;
	setSelection(selection: Array<string>): void;
}

export class UmbEntityBulkActionBase<RepositoryType = unknown> extends UmbActionBase<RepositoryType> {
	selection: Array<string>;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.selection = selection;
	}

	setSelection(selection: Array<string>) {
		this.selection = selection;
	}
}
