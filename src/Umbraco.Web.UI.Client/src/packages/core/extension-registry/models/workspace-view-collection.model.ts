import type {
	ManifestWithConditions,
	ManifestWithView,
	MetaManifestWithView,
} from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceViewCollection
	extends ManifestWithView,
		ManifestWithConditions<ConditionsEditorViewCollection> {
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

export interface ConditionsEditorViewCollection {
	/**
	 * The workspaces that this view collection should be available in
	 *
	 * @examples [
	 * "Umb.Workspace.DataType",
	 * "Umb.Workspace.Dictionary",
	 * "Umb.Workspace.Document",
	 * "Umb.Workspace.DocumentType",
	 * "Umb.Workspace.Language",
	 * "Umb.Workspace.LanguageRoot",
	 * "Umb.Workspace.LogviewerRoot",
	 * "Umb.Workspace.Media",
	 * "Umb.Workspace.MediaType",
	 * "Umb.Workspace.Member",
	 * "Umb.Workspace.MemberType",
	 * "Umb.Workspace.MemberGroup",
	 * "Umb.Workspace.Package",
	 * "Umb.Workspace.PackageBuilder",
	 * "Umb.Workspace.PartialView",
	 * "Umb.Workspace.RelationType",
	 * "Umb.Workspace.Stylesheet",
	 * "Umb.Workspace.Template"
	 * ]
	 */
	workspaces: string[];
}
