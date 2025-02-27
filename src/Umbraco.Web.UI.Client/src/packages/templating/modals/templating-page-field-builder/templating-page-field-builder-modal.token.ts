import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbTemplatingPageFieldBuilderModalData {}

export type UmbTemplatingPageFieldBuilderModalValue = {
	output: string;
};

export const UMB_TEMPLATING_PAGE_FIELD_BUILDER_MODAL = new UmbModalToken<
	UmbTemplatingPageFieldBuilderModalData,
	UmbTemplatingPageFieldBuilderModalValue
>('Umb.Modal.TemplatingPageFieldBuilder', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
