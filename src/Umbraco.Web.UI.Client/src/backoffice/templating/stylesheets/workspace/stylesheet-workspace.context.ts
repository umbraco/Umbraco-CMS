import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbStylesheetRepository } from '../repository/stylesheet.repository';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { UmbWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';

export class UmbStylesheetWorkspaceContext
	extends UmbWorkspaceContext<UmbStylesheetRepository>
	implements UmbWorkspaceContextInterface
{
	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbStylesheetRepository(host));
	}

	/*
	getEntityType(): string {
		return 'stylesheet';
	}

	getEntityKey() {
		return '1234';
	}

	getData() {
		return 'fake' as unknown as MemberDetails;
	}

	async save() {
		console.log('save');
	}

	async load(path: string) {
		console.log('load', path);
	}

	public destroy(): void {
		console.log('destroy');
	}
	*/
}
