import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplateQueryBuilderModalData {
	hidePartialViews?: boolean;
}

export interface UmbTemplateQueryBuilderModalValue {
	value: string;
}

export const UMB_TEMPLATE_QUERY_BUILDER_MODAL = new UmbModalToken<
	UmbTemplateQueryBuilderModalData,
	UmbTemplateQueryBuilderModalValue
>('Umb.Modal.Template.QueryBuilder', {
	modal: {
		type: 'sidebar',
		size: 'large',
	},
});
