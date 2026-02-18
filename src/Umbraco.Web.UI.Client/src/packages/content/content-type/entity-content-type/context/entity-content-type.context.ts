import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from './entity-content-type.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbEntityContentTypeEntityContext extends UmbContextBase {
	#entityType = new UmbStringState<string | undefined>(undefined);
	public readonly entityType = this.#entityType.asObservable();

	#unique = new UmbStringState<string | undefined>(undefined);
	public readonly unique = this.#unique.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT);
	}

	getEntityType(): string | undefined {
		return this.#entityType.getValue();
	}

	setEntityType(entityType: string | undefined): void {
		this.#entityType.setValue(entityType);
	}

	getUnique(): string | undefined {
		return this.#unique.getValue();
	}

	setUnique(unique: string | undefined): void {
		this.#unique.setValue(unique);
	}
}
