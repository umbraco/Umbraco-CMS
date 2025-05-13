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

	getAlias(): string | undefined;
	setAlias(alias: string): void;

	getCompositions(): Array<UmbContentTypeCompositionModel> | undefined;
	setCompositions(compositions: Array<UmbContentTypeCompositionModel>): void;

	getDescription(): string | undefined;
	setDescription(description: string): void;

	getIcon(): string | undefined;
	setIcon(icon: string): void;

	getName(): string | undefined;
	setName(name: string): void;
}
