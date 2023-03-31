import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbStylesheetRepository } from '../repository/stylesheet.repository';
import { StylesheetDetails } from '..';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbStylesheetWorkspaceContext extends UmbWorkspaceContext<UmbStylesheetRepository, StylesheetDetails> {
	#data = new ObjectState<StylesheetDetails | undefined>(undefined);
	data = this.#data.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbStylesheetRepository(host));
	}

	getEntityType(): string {
		return 'stylesheet';
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.path || '';
	}

	async load(path: string) {
		const { data } = await this.repository.requestByPath(path);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	public async save() {
		throw new Error('Save method not implemented.');
	}

	public destroy(): void {
		this.#data.complete();
	}
}
