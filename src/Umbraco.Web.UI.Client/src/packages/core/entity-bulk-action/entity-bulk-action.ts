import type { UmbAction} from '@umbraco-cms/backoffice/action';
import { UmbActionBase } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityBulkAction<RepositoryType = unknown> extends UmbAction<RepositoryType> {
	selection: Array<string>;
	setSelection(selection: Array<string>): void;
	execute(): Promise<void>;
}

export abstract class UmbEntityBulkActionBase<RepositoryType = unknown>
	extends UmbActionBase<RepositoryType>
	implements UmbEntityBulkAction<RepositoryType>
{
	selection: Array<string>;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, selection: Array<string>) {
		super(host, repositoryAlias);
		this.selection = selection;
	}

	setSelection(selection: Array<string>) {
		this.selection = selection;
	}

	abstract execute(): Promise<void>;
}
