import { body, defaultResponse, endpoint, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({ method: 'GET', path: '/manifests' })
export class Manifests {
	@response({ status: 200 })
	response(@body body: ManifestsResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

export type Manifest = IManifestElement | IManifestEntrypoint;
export type ManifestStandardTypes = 'section' | 'propertyEditorUI' | 'dashboard';

export interface ManifestsResponse {
	manifests: Manifest[];
}

export interface IManifest {
	type: string;
}

export interface MetaSection {
	pathname: string;
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
}

export interface IManifestElement extends IManifest {
	type: ManifestStandardTypes;
	alias: string;
	name: string;
	js: string;
	elementName: string;
	meta: MetaSection | MetaPropertyEditorUI | MetaDashboard;
}

export interface IManifestEntrypoint extends IManifest {
	type: 'entrypoint';
	js: string;
}
