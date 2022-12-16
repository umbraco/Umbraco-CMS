
import type { ManifestHeaderApp } from './header-app.models';
import type { ManifestSection } from './section.models';
import type { ManifestSectionView } from './section-view.models';
import type { ManifestTree } from './tree.models';
import type { ManifestTreeItemAction } from './tree-item-action.models';
import type { ManifestEditor } from './editor.models';
import type { ManifestEditorAction } from './editor-action.models';
import type { ManifestEditorView } from './editor-view.models';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.models';
import type { ManifestDashboard } from './dashboard.models';
import type { ManifestUserDashboard } from './user-dashboard.models';
import type { ManifestPropertyAction } from './property-action.models';
import type { ManifestPackageView } from './package-view.models';
import type { ManifestExternalLoginProvider } from './external-login-provider.models';

export * from './header-app.models';
export * from './section.models';
export * from './section-view.models';
export * from './tree.models';
export * from './tree-item-action.models';
export * from './editor.models';
export * from './editor-action.models';
export * from './editor-view.models';
export * from './property-editor.models';
export * from './dashboard.models';
export * from './user-dashboard.models';
export * from './property-action.models';
export * from './package-view.models';
export * from './external-login-provider.models';

export type ManifestTypes =
	| ManifestHeaderApp
	| ManifestSection
	| ManifestSectionView
	| ManifestTree
	| ManifestEditor
	| ManifestEditorAction
	| ManifestEditorView
	| ManifestTreeItemAction
	| ManifestPropertyEditorUI
	| ManifestPropertyEditorModel
	| ManifestDashboard
	| ManifestUserDashboard
	| ManifestPropertyAction
	| ManifestPackageView
	| ManifestExternalLoginProvider
	| ManifestEntrypoint
	| ManifestCustom;

export type ManifestStandardTypes =
	| 'headerApp'
	| 'section'
	| 'sectionView'
	| 'tree'
	| 'editor'
	| 'editorView'
	| 'editorAction'
	| 'treeItemAction'
	| 'propertyEditorUI'
	| 'propertyEditorModel'
	| 'dashboard'
	| 'user-dashboard'
	| 'propertyAction'
	| 'packageView'
	| 'entrypoint'
	| 'externalLoginProvider';

export type ManifestElementType =
	| ManifestSection
	| ManifestSectionView
	| ManifestTree
	| ManifestTreeItemAction
	| ManifestEditor
	| ManifestPropertyAction
	| ManifestPropertyEditorUI
	| ManifestDashboard
	| ManifestUserDashboard
	| ManifestEditorView
	| ManifestEditorAction
	| ManifestPackageView
	| ManifestExternalLoginProvider;

export interface ManifestBase {
	type: string;
	alias: string;
	name: string;
	weight?: number;
}

export interface ManifestElement extends ManifestBase {
	type: ManifestStandardTypes;
	js?: string;
	elementName?: string;
	loader?: () => Promise<object | HTMLElement>;
	meta?: any;
}

export interface ManifestCustom extends ManifestBase {
	type: 'custom';
	meta?: any;
}

export interface ManifestEntrypoint extends ManifestBase {
	type: 'entrypoint';
	js: string;
}
