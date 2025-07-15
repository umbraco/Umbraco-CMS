import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreatePartialViewFromSnippetModalData {
	parent: UmbEntityModel;
}

export const UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL = new UmbModalToken<UmbCreatePartialViewFromSnippetModalData, string>(
	'Umb.Modal.PartialView.CreateFromSnippet',
	{
		modal: {
			type: 'sidebar',
		},
	},
);
