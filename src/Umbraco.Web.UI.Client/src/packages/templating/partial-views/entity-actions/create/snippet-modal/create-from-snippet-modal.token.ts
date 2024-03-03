import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreatePartialViewFromSnippetModalData {
	parent: {
		unique: string | null;
		entityType: string;
	};
}

export const UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL = new UmbModalToken<UmbCreatePartialViewFromSnippetModalData, string>(
	'Umb.Modal.PartialView.CreateFromSnippet',
	{
		modal: {
			type: 'sidebar',
		},
	},
);
