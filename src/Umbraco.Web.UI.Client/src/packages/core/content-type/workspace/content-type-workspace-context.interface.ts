import type { UmbContentTypeCompositionModel, UmbContentTypeModel, UmbContentTypeSortModel } from '../types.js';
import type { UmbContentTypeStructureManager } from '../structure/index.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export interface UmbContentTypeWorkspaceContext<ContentTypeType extends UmbContentTypeModel = UmbContentTypeModel>
	extends UmbSubmittableWorkspaceContext {
	readonly IS_CONTENT_TYPE_WORKSPACE_CONTEXT: true;

	readonly name: Observable<string | undefined>;
	readonly alias: Observable<string | undefined>;
	readonly description: Observable<string | undefined>;
	readonly icon: Observable<string | undefined>;

	readonly allowedAtRoot: Observable<boolean | undefined>;
	readonly variesByCulture: Observable<boolean | undefined>;
	readonly variesBySegment: Observable<boolean | undefined>;
	//readonly isElement: Observable<boolean | undefined>;
	readonly allowedContentTypes: Observable<UmbContentTypeSortModel[] | undefined>;
	readonly compositions: Observable<UmbContentTypeCompositionModel[] | undefined>;

	readonly structure: UmbContentTypeStructureManager<ContentTypeType>;

	setAlias(alias: string): void;
	setCompositions(compositions: Array<UmbContentTypeCompositionModel>): void;
	setDescription(description: string): void;
	setIcon(icon: string): void;
	setName(name: string): void;
}
