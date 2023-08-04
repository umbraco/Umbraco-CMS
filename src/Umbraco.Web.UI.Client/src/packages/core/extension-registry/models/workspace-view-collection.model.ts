import { ConditionTypes } from '../conditions/types.js';
import type {
	ManifestWithDynamicConditions,
	ManifestWithView,
	MetaManifestWithView,
} from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceViewCollection
	extends ManifestWithView,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceViewCollection';
	meta: MetaEditorViewCollection;
}

export interface MetaEditorViewCollection extends MetaManifestWithView {
	/**
	 * The entity type that this view collection should be available in
	 *
	 * @examples [
	 * "media"
	 * ]
	 */
	entityType: string;

	/**
	 * The repository alias that this view collection should be available in
	 * @examples [
	 * "Umb.Repository.Media"
	 * ]
	 */
	repositoryAlias: string;
}
