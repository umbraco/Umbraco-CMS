import { body, defaultResponse, endpoint, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({ method: 'GET', path: '/manifests' })
export class Manifests {
	@response({ status: 200 })
	response(@body body: ManifestsResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

export type Manifest =
	| IManifestSection
	| IManifestTree
	| IManifestEditor
	| IManifestEntityAction
	| IManifestPropertyEditorUI
	| IManifestDashboard
	| IManifestEditorView
	| IManifestPropertyAction
	| IManifestEntrypoint
	| IManifestCustom;

export type ManifestStandardTypes =
	| 'section'
	| 'tree'
	| 'editor'
	| 'entityAction'
	| 'propertyEditorUI'
	| 'dashboard'
	| 'editorView'
	| 'propertyAction'
	| 'entrypoint';

export interface ManifestsResponse {
	manifests: Manifest[];
}

export interface IManifest {
	type: string;
	alias: string;
}

export interface MetaSection {
	pathname: string;
	weight: number;
}

export interface MetaTree {
	weight: number;
	sections: Array<string>;
}

export interface MetaEditor {
	entityType: string;
}

export interface MetaEntityAction {
	label: string;
	icon: string;
	weight: number;
}

export interface MetaPropertyEditorUI {
	icon: string;
	group: string;
}

export interface MetaDashboard {
	sections: string[];
	pathname: string;
	weight: number;
	label?: string;
}

export interface MetaEditorView {
	editors: string[];
	pathname: string;
	weight: number;
	icon: string;
}

export interface MetaPropertyAction {
	propertyEditors: string[];
}

export interface IManifestCustom extends IManifest {
	type: 'custom';
	meta?: {};
}

export interface IManifestElement extends IManifest {
	type: ManifestStandardTypes;
	name: string;
	js?: string;
	elementName?: string;
	meta?: {};
}

export interface IManifestSection extends IManifestElement {
	type: 'section';
	meta: MetaSection;
}

export interface IManifestTree extends IManifestElement {
	type: 'tree';
	meta: MetaTree;
}

export interface IManifestEditor extends IManifestElement {
	type: 'editor';
	meta: MetaEditor;
}

export interface IManifestEntityAction extends IManifestElement {
	type: 'entityAction';
	meta: MetaEntityAction;
}

export interface IManifestPropertyEditorUI extends IManifestElement {
	type: 'propertyEditorUI';
	meta: MetaPropertyEditorUI;
}

export interface IManifestDashboard extends IManifestElement {
	type: 'dashboard';
	meta: MetaDashboard;
}

export interface IManifestEditorView extends IManifestElement {
	type: 'editorView';
	meta: MetaEditorView;
}

export interface IManifestPropertyAction extends IManifestElement {
	type: 'propertyAction';
	meta: MetaPropertyAction;
}

export interface IManifestEntrypoint extends IManifest {
	type: 'entrypoint';
	js: string;
}
