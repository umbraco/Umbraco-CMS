import type { UmbItemModel } from '../types.js';
import type { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbItemDataResolverConstructor<ItemType extends UmbItemModel = UmbItemModel> {
	new (host: UmbControllerHost): UmbItemDataResolver<ItemType>;
}

export interface UmbItemDataResolver<ItemType extends UmbItemModel = UmbItemModel> extends UmbControllerBase {
	entityType: Observable<string | undefined>;
	unique: Observable<string | undefined>;
	name: Observable<string | undefined>;
	icon: Observable<string | undefined>;

	setData(data: ItemType | undefined): void;
	getData(): ItemType | undefined;

	getEntityType(): Promise<string | undefined>;
	getUnique(): Promise<string | undefined>;
	getName(): Promise<string | undefined>;
	getIcon(): Promise<string | undefined>;
}
