import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

/**
 * UmbEntityContext
 * @export
 * @class UmbEntityContext
 * @extends {UmbContextBase<UmbEntityContext>}
 */
export class UmbEntityContext extends UmbContextBase<UmbEntityContext> {
	#entityType = new UmbStringState<string | undefined>(undefined);
	public readonly entityType = this.#entityType.asObservable();

	#unique = new UmbStringState<string | null | undefined>(undefined);
	public readonly unique = this.#unique.asObservable();

	/**
	 * Creates an instance of UmbEntityContext.
	 * @param {UmbControllerHost} host
	 * @memberof UmbEntityContext
	 */
	constructor(host: UmbControllerHost) {
		super(host, 'entity');
	}

	setEntityType(entityType: string | undefined) {
		this.#entityType.setValue(entityType);
	}

	getEntityType() {
		return this.#entityType.getValue();
	}

	setUnique(unique: string | null | undefined) {
		this.#unique.setValue(unique);
	}

	getUnique() {
		return this.#unique.getValue();
	}
}
