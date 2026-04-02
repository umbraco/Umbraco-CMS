import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

// Omitting the unique because we don't need it for flags and its causes trouble because it can be both null and a string
export interface UmbEntityWithFlags extends Omit<UmbEntityModel, 'unique'> {
	flags: Array<UmbEntityFlag>;
}

// Omitting the unique because we don't need it for flags and its causes trouble because it can be both null and a string
export interface UmbEntityWithOptionalFlags extends Omit<UmbEntityModel, 'unique'> {
	flags?: UmbEntityWithFlags['flags'];
}

export interface UmbEntityFlag {
	alias: string;
}
