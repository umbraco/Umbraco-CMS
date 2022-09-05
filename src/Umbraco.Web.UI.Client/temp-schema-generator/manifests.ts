import { body, defaultResponse, endpoint, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({ method: 'GET', path: '/manifests' })
export class Manifests {
	@response({ status: 200 })
	response(@body body: ManifestsResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({ method: 'GET', path: '/manifests/packages' })
export class Packages {
	@response({ status: 200 })
	response(@body body: PackagesResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

export type Manifest =
	| IManifestSection
	| IManifestPropertyEditorUI
	| IManifestDashboard
	| IManifestEditorView
	| IManifestPropertyAction
	| IManifestPackageView
	| IManifestEntrypoint
	| IManifestCustom;

export type ManifestStandardTypes =
	| 'section'
	| 'propertyEditorUI'
	| 'dashboard'
	| 'editorView'
	| 'propertyAction'
	| 'packageView'
	| 'entrypoint';

export interface ManifestsResponse {
	manifests: Manifest[];
}

export interface PackagesResponse {
	packages: Package[];
}

export interface Package {
	name: string;
	alias: string;
	version: string;
}

export interface IManifest {
	type: string;
	alias: string;
}

export interface IPrevalueField {
	label?: string;
	description?: string;
	key: string;
	view: string;
}

export interface IPrevalues {
	prevalues?: {
		fields: IPrevalueField[];
	};
	defaultConfig?: {};
}

export interface MetaSection {
	pathname: string;
	weight: number;
}

export interface MetaPropertyEditorUI extends IPrevalues {
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

export interface MetaPackageView {
	packageAlias: string;
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

export interface IManifestPackageView extends IManifestElement {
	type: 'packageView';
	meta: MetaPackageView;
}

export interface IManifestEntrypoint extends IManifest {
	type: 'entrypoint';
	js: string;
}
