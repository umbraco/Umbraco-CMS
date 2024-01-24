import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreatePartialViewFromSnippetModalData {
	parentUnique: string | null;
}

export const UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL = new UmbModalToken<UmbCreatePartialViewFromSnippetModalData, string>(
	'Umb.Modal.PartialView.CreateFromSnippet',
	{
		modal: {
			type: 'sidebar',
		},
	},
);
