import { UMB_SCHEMA_LOCKDOWN_CONTEXT } from './schema-lockdown.context-token.js';
import type { UmbSchemaLockdownContext } from './schema-lockdown.context.js';
import type { UmbSchemaOperation, UmbSchemaOperationOrCreateOrUpdate } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SUBMITTABLE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

export interface UmbSchemaOperationAllowedControllerArgs {
	/**
	 * The backoffice entity type to check. Can also be set later with `setEntityType`.
	 */
	entityType?: string;

	/**
	 * The operation to check.
	 */
	operation: UmbSchemaOperationOrCreateOrUpdate;
}

/**
 * Observes whether a schema operation is allowed for an entity type.
 * The 'createOrUpdate' operation follows the surrounding workspace: it checks 'create' while the workspace holds a new
 * entity, and 'update' otherwise. The operation counts as allowed until the entity type is known.
 */
export class UmbSchemaOperationAllowedController extends UmbControllerBase {
	#allowed = new UmbBooleanState(true);
	readonly allowed = this.#allowed.asObservable();

	#schemaLockdownContext?: UmbSchemaLockdownContext;
	#entityType?: string;
	#operation: UmbSchemaOperationOrCreateOrUpdate;
	#isNew?: boolean;

	constructor(host: UmbControllerHost, args: UmbSchemaOperationAllowedControllerArgs) {
		super(host);
		this.#entityType = args.entityType;
		this.#operation = args.operation;

		this.consumeContext(UMB_SCHEMA_LOCKDOWN_CONTEXT, (context) => {
			this.#schemaLockdownContext = context;
			this.observe(context?.state, () => this.#update(), '_observeSchemaLockdown');
		});

		if (this.#operation === 'createOrUpdate') {
			this.consumeContext(UMB_SUBMITTABLE_WORKSPACE_CONTEXT, (context) => {
				this.observe(
					context?.isNew,
					(isNew) => {
						this.#isNew = isNew;
						this.#update();
					},
					'_observeIsNew',
				);
			});
		}
	}

	/**
	 * Sets the backoffice entity type to check.
	 * @param {string | undefined} entityType The backoffice entity type.
	 */
	setEntityType(entityType: string | undefined) {
		this.#entityType = entityType;
		this.#update();
	}

	/**
	 * Whether the operation is currently allowed.
	 * @returns {boolean} Whether the operation is allowed.
	 */
	getAllowed(): boolean {
		return this.#allowed.getValue();
	}

	#update() {
		if (!this.#schemaLockdownContext || !this.#entityType) {
			this.#allowed.setValue(true);
			return;
		}
		this.#allowed.setValue(this.#schemaLockdownContext.isAllowed(this.#entityType, this.#resolveOperation()));
	}

	#resolveOperation(): UmbSchemaOperation {
		if (this.#operation !== 'createOrUpdate') return this.#operation;
		return this.#isNew ? 'create' : 'update';
	}

	override destroy() {
		// Destroying the consumers below calls back into #update, so the state has to outlive them.
		super.destroy();
		this.#allowed.destroy();
	}
}
