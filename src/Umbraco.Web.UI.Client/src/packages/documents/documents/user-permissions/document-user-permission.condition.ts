import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_CURRENT_USER_CONTEXT } from '../../../user/current-user/current-user.context.js';
import { isDocumentUserPermission } from './utils.js';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentPermissionPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

export class UmbDocumentUserPermissionCondition
	extends UmbConditionBase<UmbDocumentUserPermissionConditionConfig>
	implements UmbExtensionCondition
{
	#entityType: string | undefined;
	#unique: string | null | undefined;
	#documentPermissions: Array<DocumentPermissionPresentationModel> = [];
	#fallbackPermissions: string[] = [];

	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbDocumentUserPermissionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(
				context.currentUser,
				(currentUser) => {
					this.#documentPermissions = currentUser?.permissions?.filter(isDocumentUserPermission) || [];
					this.#fallbackPermissions = currentUser?.fallbackPermissions || [];
					this.#isAllowed();
				},
				'umbUserPermissionConditionObserver',
			);
		});

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;

			this.observe(
				observeMultiple([context.entityType, context.unique]),
				([entityType, unique]) => {
					this.#entityType = entityType;
					this.#unique = unique;
					this.#isAllowed();
				},
				'umbUserPermissionEntityContextObserver',
			);
		});
	}

	#isAllowed() {
		if (!this.#entityType) return;
		if (this.#unique === undefined) return;

		const hasDocumentPermissions = this.#documentPermissions.length > 0;

		// if there is no permissions for any documents we use the fallback permissions
		if (!hasDocumentPermissions) {
			this.#check(this.#fallbackPermissions);
			return;
		}

		if (hasDocumentPermissions) {
			const permissionsForCurrentDocument = this.#documentPermissions.find(
				(permission) => permission.document.id === this.#unique,
			);

			// no permissions for the current document - use the fallback permissions
			if (!permissionsForCurrentDocument) {
				this.#check(this.#fallbackPermissions);
				return;
			}

			this.#check(permissionsForCurrentDocument.verbs);
		}
	}

	#check(verbs: Array<string>) {
		this.permitted = this.config.allOf.every((verb) => verbs.includes(verb));
	}
}

export type UmbDocumentUserPermissionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.UserPermission.Document'> & {
		/**
		 *
		 *
		 * @example
		 * ["Umb.Document.Save", "Umb.Document.Publish"]
		 */
		allOf: Array<string>;
	};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Document User Permission Condition',
	alias: 'Umb.Condition.UserPermission.Document',
	api: UmbDocumentUserPermissionCondition,
};
