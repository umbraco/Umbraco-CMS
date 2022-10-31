import type { ManifestSection } from './section.models';
import type { ManifestSectionView } from './section-view.models';
import type { ManifestTree } from './tree.models';
import type { ManifestTreeItemAction } from './tree-item-action.models';
import type { ManifestEditor } from './editor.models';
import type { ManifestEditorAction } from './editor-action.models';
import type { ManifestEditorView } from './editor-view.models';
import type { ManifestPropertyEditorUI, ManifestPropertyEditorModel } from './property-editor.models';
import type { ManifestDashboard } from './dashboard.models';
import type { ManifestPropertyAction } from './property-action.models';
import type { ManifestPackageView } from './package-view.models';

export * from './section.models';
export * from './section-view.models';
export * from './tree.models';
export * from './tree-item-action.models';
export * from './editor.models';
export * from './editor-action.models';
export * from './editor-view.models';
export * from './property-editor.models';
export * from './dashboard.models';
export * from './property-action.models';
export * from './package-view.models';

export type ManifestTypes =
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
	| ManifestPropertyAction
	| ManifestPackageView
	| ManifestEntrypoint
	| ManifestCustom;

export type ManifestStandardTypes =
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
	| 'propertyAction'
	| 'packageView'
	| 'entrypoint';

export type ManifestElementType =
	| ManifestSection
	| ManifestSectionView
	| ManifestTree
	| ManifestTreeItemAction
	| ManifestEditor
	| ManifestPropertyAction
	| ManifestPropertyEditorUI
	| ManifestDashboard
	| ManifestEditorView
	| ManifestEditorAction
	| ManifestPackageView;

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

// TODO: couldn't we make loader optional on all manifests? and not just the internal ones?
export type ManifestWithLoader<T> = T & { loader: () => Promise<object | HTMLElement> };
