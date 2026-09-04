import type { UmbContentPickerDynamicRoot } from '../types.js';
import { UmbContentPickerDynamicRootRepository } from './repository/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_PARENT_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

/**
 * Resolves a configured dynamic root to the node a picker should start from.
 *
 * A dynamic root is resolved against the content being edited rather than against the data type, so resolving it
 * needs the workspace and parent context a picker happens to be rendered in. Any picker that offers a dynamic root
 * needs exactly that, which is why this is a controller rather than part of one picker.
 */
export class UmbDynamicRootResolver extends UmbControllerBase {
	readonly #repository = new UmbContentPickerDynamicRootRepository(this);

	/**
	 * Resolves the unique of the node the picker should start from.
	 * @param {UmbContentPickerDynamicRoot | undefined} dynamicRoot - The configured dynamic root.
	 * @returns {Promise<string | undefined>} The resolved start node unique, or undefined when there is nothing to resolve.
	 */
	async resolveStartNodeUnique(dynamicRoot: UmbContentPickerDynamicRoot | undefined): Promise<string | undefined> {
		if (!dynamicRoot) return undefined;

		// Use passContextAliasMatches to skip past block element workspaces and find the document workspace.
		const workspaceContext = await this.getContext(UMB_CONTENT_WORKSPACE_CONTEXT, {
			passContextAliasMatches: true,
		}).catch(() => undefined);

		// For new documents, the unique is a client-generated GUID that doesn't exist in the DB.
		// The backend expects null for CurrentKey when creating new content and falls back to ParentKey.
		const isNew =
			workspaceContext &&
			'getIsNew' in workspaceContext &&
			(workspaceContext as UmbSubmittableWorkspaceContext).getIsNew() === true;

		const unique = isNew ? null : (workspaceContext?.getUnique() ?? null);

		// Use parent entity context to get the parent unique. Its observable starts as undefined,
		// so asPromise() properly waits for the async structure loading to complete.
		const parentContext = await this.getContext(UMB_PARENT_ENTITY_CONTEXT);
		const parent = await this.observe(parentContext?.parent, () => {})?.asPromise();
		const parentUnique = parent?.unique ?? null;

		const result = await this.#repository.requestRoot(dynamicRoot, unique, parentUnique);

		return result?.length ? result[0] : undefined;
	}
}
